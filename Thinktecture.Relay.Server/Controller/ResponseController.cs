using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Controller
{
	[Authorize(Roles = "OnPremise")]
	[OnPremiseConnectionModuleBindingFilter]
	[NoCache]
	public class ResponseController : ApiController
	{
		private readonly ILogger _logger;
		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly IBackendCommunication _backendCommunication;

		public ResponseController(IBackendCommunication backendCommunication, ILogger logger, IPostDataTemporaryStore postDataTemporaryStore)
		{
			_logger = logger;
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
		}

		internal class LegacyResponseStream : MemoryStream
		{
			private static readonly byte[] _bodyStart = Encoding.UTF8.GetBytes("\"Body\":\"");
			private static readonly byte[] _bodyEnd = Encoding.UTF8.GetBytes("\"");

			private readonly Stream _sourceStream;
			private readonly IPostDataTemporaryStore _postDataTemporaryStore;
			private readonly ILogger _logger;
			private readonly byte[] _buffer;

			public LegacyResponseStream(Stream sourceStream, IPostDataTemporaryStore postDataTemporaryStore, ILogger logger, int bufferSize = 0x10000)
			{
				_sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
				_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
				_logger = logger;
				_buffer = new byte[bufferSize];
			}

			public async Task<string> ExtractBodyAsync()
			{
				var temporaryId = Guid.NewGuid().ToString();
				_logger?.Verbose("Extracting body from legacy on-premise response. temporary-id={TemporaryId}", temporaryId);

				var streamPosition = 0;
				var offset = 0;
				var overlap = 0;

				while (true)
				{
					var length = await _sourceStream.ReadAsync(_buffer, offset, _buffer.Length - offset).ConfigureAwait(false);
					if (length == 0)
					{
						// nothing more to read but overlapping bytes should be written finally
						await WriteAsync(_buffer, 0, overlap).ConfigureAwait(false);
						break;
					}

					streamPosition += length;

					length += offset;

					var position = Search(_buffer, length, _bodyStart);
					if (position != 0)
					{
						_logger?.Verbose("Found body value start in legacy on-premise response. temporary-id={TemporaryId}, body-start={BodyStart}", temporaryId, streamPosition - length + position);

						await WriteAsync(_buffer, 0, position).ConfigureAwait(false);

						length -= position;
						Buffer.BlockCopy(_buffer, position, _buffer, 0, length);
						offset = 0;

						var decoder = new SimpleBase64InplaceDecoder();
						int decoded;

						using (var stream = _postDataTemporaryStore.CreateResponseStream(temporaryId))
						{
							while (true)
							{
								length += offset;

								position = Search(_buffer, length, _bodyEnd);
								if (position != 0)
								{
									_logger?.Verbose("Found body value end in legacy on-premise response. temporary-id={TemporaryId}, body-end={BodyEnd}", temporaryId, streamPosition - length + position);

									position -= _bodyEnd.Length;

									decoded = decoder.Decode(_buffer, position, out offset);
									if (offset != position)
										throw new InvalidOperationException("Unexpected end of base64 response.");

									await stream.WriteAsync(_buffer, 0, decoded).ConfigureAwait(false);

									await WriteAsync(_buffer, position, length - position).ConfigureAwait(false);
									break;
								}


								decoded = decoder.Decode(_buffer, length, out offset);
								await stream.WriteAsync(_buffer, 0, decoded).ConfigureAwait(false);

								if (offset != length)
								{
									Buffer.BlockCopy(_buffer, offset, _buffer, 0, length - offset);
								}

								offset = length - offset;

								length = await _sourceStream.ReadAsync(_buffer, offset, _buffer.Length - offset).ConfigureAwait(false);
								if (length == 0)
									throw new InvalidOperationException("Unexpected end of base64 response.");

								streamPosition += length;
							}
						}

						while (true)
						{
							length = await _sourceStream.ReadAsync(_buffer, 0, _buffer.Length).ConfigureAwait(false);
							if (length == 0)
							{
								break;
							}

							await WriteAsync(_buffer, 0, length).ConfigureAwait(false);
						}

						break;
					}

					// keep some overlapping bytes
					overlap = Math.Max(length - _bodyStart.Length, 0);
					offset = Math.Min(_bodyStart.Length, length);

					await WriteAsync(_buffer, 0, length - offset).ConfigureAwait(false);

					Buffer.BlockCopy(_buffer, overlap, _buffer, 0, offset);
				}

				// rewind
				Position = 0;

				return temporaryId;
			}

			private int Search(byte[] buffer, int count, byte[] pattern)
			{
				var position = 0;

				for (var i = 0; i < count; i++)
				{
					if (buffer[i] != pattern[position])
					{
						position = 0;
					}

					if (buffer[i] == pattern[position])
					{
						if (++position == pattern.Length)
						{
							return i + 1;
						}
					}
				}

				return 0;
			}
		}

		internal class SimpleBase64InplaceDecoder
		{
			private const byte _FILL_BYTE = (byte)'=';

			private static readonly byte[] _base64 = Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/");
			private static readonly byte[] _decode = new byte[256];

			static SimpleBase64InplaceDecoder()
			{
				for (var i = 0; i < _base64.Length; i++)
				{
					_decode[_base64[i]] = (byte)i;
				}
			}

			public int Decode(byte[] buffer, int count, out int offset)
			{
				var index = 0;
				offset = count / 4 * 4;

				for (var i = 0; i < offset; i += 4)
				{
					var triple = _decode[buffer[i]] << 18 | _decode[buffer[i + 1]] << 12 | _decode[buffer[i + 2]] << 6 | _decode[buffer[i + 3]];

					buffer[index++] = (byte)(triple >> 16);

					if (buffer[i + 2] != _FILL_BYTE)
					{
						buffer[index++] = (byte)(triple >> 8);

						if (buffer[i + 3] != _FILL_BYTE)
						{
							buffer[index++] = (byte)triple;
						}
					}
				}

				return index;
			}
		}

		public async Task<IHttpActionResult> Forward()
		{
			var requestStream = await Request.Content.ReadAsStreamAsync().ConfigureAwait(false);

			OnPremiseConnectorResponse response = null;
			if (Request.Headers.TryGetValues("X-TTRELAY-METADATA", out var headerValues))
			{
				response = JToken.Parse(headerValues.First()).ToObject<OnPremiseConnectorResponse>();

				using (var stream = _postDataTemporaryStore.CreateResponseStream(response.RequestId))
				{
					await requestStream.CopyToAsync(stream).ConfigureAwait(false);
					response.ContentLength = stream.Length;
				}
			}
			else
			{
				// this is a legacy on-premise connector (v1)
				response = await ForwardLegacyResponse(Encoding.UTF8, requestStream).ConfigureAwait(false);
			}

			_logger?.Verbose("Received on-premise response. request-id={RequestId}, content-length={ResponseContentLength}", response.RequestId, response.ContentLength);

			await _backendCommunication.SendOnPremiseTargetResponseAsync(response.OriginId, response).ConfigureAwait(false);

			return Ok();
		}

		private async Task<OnPremiseConnectorResponse> ForwardLegacyResponse(Encoding encoding, Stream requestStream)
		{
			using (var stream = new LegacyResponseStream(requestStream, _postDataTemporaryStore, _logger))
			{
				var temporaryId = await stream.ExtractBodyAsync().ConfigureAwait(false);

				using (var reader = new JsonTextReader(new StreamReader(stream)))
				{
					var message = await JToken.ReadFromAsync(reader).ConfigureAwait(false);
					var response = message.ToObject<OnPremiseConnectorResponse>();

					response.Body = null;
					response.ContentLength = _postDataTemporaryStore.RenameResponseStream(temporaryId, response.RequestId);

					_logger?.Verbose("Extracted legacy on-premise response. request-id={RequestId}", response.RequestId);

					return response;
				}
			}
		}
	}
}
