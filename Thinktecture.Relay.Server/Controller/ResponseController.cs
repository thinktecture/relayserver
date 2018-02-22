using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

		internal class LegacyResponseReader : TextReader
		{
			private const int _OVERLAP = 16;

			private static readonly char[] _bodyStart = "\"Body\":\"".ToCharArray();
			private static readonly char[] _bodyEnd = "\"".ToCharArray();
			private static readonly char[] _bodyReplace = "null".ToCharArray();

			private readonly StreamReader _reader;
			private readonly IPostDataTemporaryStore _postDataTemporaryStore;
			private readonly ILogger _logger;
			private readonly int _bufferSize;
			private readonly char[] _overlapBuffer = new char[_OVERLAP];

			private int _overlapBufferLength;
			private char[] _partialBuffer;
			private int _partialBufferIndex;
			private int _partialBufferLength;

			public string TemporaryId { get; private set; }

			public LegacyResponseReader(Stream stream, Encoding encoding, IPostDataTemporaryStore postDataTemporaryStore, ILogger logger, int bufferSize = 0x10000)
			{
				if (stream == null)
					throw new ArgumentNullException(nameof(stream));

				_reader = new StreamReader(stream, encoding, true, bufferSize, true);
				_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
				_logger = logger;
				_bufferSize = bufferSize;
			}

			public override void Close()
			{
				_reader.Close();
				base.Close();
			}

			public override int Peek()
			{
				throw new NotSupportedException();
			}

			public override int Read()
			{
				throw new NotSupportedException();
			}

			public override int Read(char[] buffer, int index, int count)
			{
				if (_partialBufferLength > 0)
				{
					var partialBufferLength = Math.Min(count, _partialBufferLength);

					CharBlockCopy(_partialBuffer, _partialBufferIndex, buffer, index, partialBufferLength);

					_partialBufferIndex += partialBufferLength;
					_partialBufferLength -= partialBufferLength;

					return partialBufferLength;
				}

				var length = _reader.Read(buffer, index, count);

				if (_partialBuffer == null && length != 0)
				{
					var start = Search(buffer, index, length, _bodyStart);
					if (start != 0)
					{
						_logger?.Verbose("Body start in legacy on-premise response found. buffer-length={BufferLength}", buffer.Length);

						ExtractBase64BodyToStore(buffer, index, length, start);
						return start;
					}

					_overlapBufferLength = Math.Min(_overlapBuffer.Length, length);
					CharBlockCopy(buffer, index + Math.Max(0, length - _overlapBuffer.Length), _overlapBuffer, 0, _overlapBufferLength);
				}

				return length;
			}

			private void CharBlockCopy(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
			{
				Buffer.BlockCopy(src, srcOffset * sizeof(char), dst, dstOffset * sizeof(char), count * sizeof(char));
			}

			private int Search(char[] buffer, int index, int count, char[] match, bool includeOverlap = true)
			{
				var position = 0;

				if (includeOverlap)
				{
					for (var o = 0; o < _overlapBuffer.Length && _overlapBuffer[o] != 0; o++)
					{
						if (_overlapBuffer[o] != match[position])
						{
							position = 0;
						}
						if (_overlapBuffer[o] == match[position])
						{
							position++;
						}
					}
				}

				for (var b = index; b < index + count; b++)
				{
					if (buffer[b] != match[position])
					{
						position = 0;
					}

					if (buffer[b] == match[position])
					{
						if (++position == match.Length)
						{
							return b + 1;
						}
					}
				}

				return 0;
			}

			private void ExtractBase64BodyToStore(char[] buffer, int index, int length, int position)
			{
				TemporaryId = Guid.NewGuid().ToString();

				_partialBuffer = new char[Math.Max(_bufferSize, length)];

				length -= position;
				CharBlockCopy(buffer, position, _partialBuffer, 0, length);

				using (var stream = _postDataTemporaryStore.CreateResponseStream(TemporaryId))
				using (var decoder = new CryptoStream(stream, new FromBase64Transform(), CryptoStreamMode.Write))
				using (var writer = new StreamWriter(decoder, Encoding.ASCII, _bufferSize, true))
				{
					while (true)
					{
						_partialBufferIndex = Search(_partialBuffer, 0, length, _bodyEnd, false);
						if (_partialBufferIndex != 0)
						{
							_logger?.Verbose("Body end in legacy on-premise response found. partial-buffer-length={PartialBufferLength}", _partialBuffer.Length);
							_partialBufferIndex--;

							writer.Write(_partialBuffer, 0, _partialBufferIndex);
							_partialBufferLength = length - _partialBufferIndex;
							break;
						}

						writer.Write(_partialBuffer, 0, length);

						length = _reader.Read(_partialBuffer, 0, _partialBuffer.Length);
						if (length == 0)
						{
							break;
						}
					}
				}
			}

			public override Task<int> ReadAsync(char[] buffer, int index, int count)
			{
				throw new NotSupportedException();
			}

			public override int ReadBlock(char[] buffer, int index, int count)
			{
				throw new NotSupportedException();
			}

			public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
			{
				throw new NotSupportedException();
			}

			public override string ReadLine()
			{
				throw new NotSupportedException();
			}

			public override Task<string> ReadLineAsync()
			{
				throw new NotSupportedException();
			}

			public override string ReadToEnd()
			{
				throw new NotSupportedException();
			}

			public override Task<string> ReadToEndAsync()
			{
				throw new NotSupportedException();
			}

			protected override void Dispose(bool disposing)
			{
				_reader.Dispose();
				base.Dispose(disposing);
			}
		}

		internal class LegacyResponseStream : MemoryStream
		{
			private static readonly byte[] _bodyStart = Encoding.UTF8.GetBytes("\"Body\":\"");
			private static readonly byte[] _bodyEnd = Encoding.UTF8.GetBytes("\"");

			private readonly Stream _sourceStream;
			private readonly IPostDataTemporaryStore _postDataTemporaryStore;
			private readonly ILogger _logger;
			private readonly byte[] _buffer;

			public LegacyResponseStream(Stream sourceStream, IPostDataTemporaryStore postDataTemporaryStore, ILogger logger, int bufferSize = 0x100000)
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
									await stream.WriteAsync(_buffer, 0, position).ConfigureAwait(false);

									await WriteAsync(_buffer, position, length - position).ConfigureAwait(false);
									break;
								}

								// keep some overlapping bytes
								overlap = Math.Max(length - _bodyEnd.Length, 0);
								offset = Math.Min(_bodyEnd.Length, length);

								await stream.WriteAsync(_buffer, 0, length - offset).ConfigureAwait(false);

								Buffer.BlockCopy(_buffer, overlap, _buffer, 0, offset);

								length = await _sourceStream.ReadAsync(_buffer, offset, _buffer.Length - offset).ConfigureAwait(false);
								if (length == 0)
									throw new InvalidOperationException("Unexpected end of base64 response.");

								streamPosition += length;
							}
						}

						await _sourceStream.CopyToAsync(this).ConfigureAwait(false);
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

			public async Task<long> DecodeBase64Response(string temporaryId, string requestId)
			{
				_logger?.Verbose("Decoding base64 body from legacy on-premise response. temporary-id={TemporaryId}, request-id={RequestId}", temporaryId, requestId);

				using (var transform = new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces))
				{
					var outputBuffer = new byte[transform.OutputBlockSize];

					using (var input = _postDataTemporaryStore.GetResponseStream(temporaryId))
					{
						using (var output = _postDataTemporaryStore.CreateResponseStream(requestId))
						{
							var offset = 0;
							while (true)
							{
								var length = await input.ReadAsync(_buffer, offset, _buffer.Length - offset).ConfigureAwait(false);
								if (length == 0)
								{
									outputBuffer = transform.TransformFinalBlock(_buffer, 0, offset);
									output.Write(outputBuffer, 0, outputBuffer.Length);
									break;
								}

								var block = 0;
								while (length - block > 4)
								{
									var transformed = transform.TransformBlock(_buffer, block, 4, outputBuffer, 0);
									await output.WriteAsync(outputBuffer, 0, transformed).ConfigureAwait(false);
									block += 4;
								}

								offset = Math.Max(0, length -= block);
								if (length > 0)
								{
									Buffer.BlockCopy(_buffer, block, _buffer, 0, length);
								}
							}

							return output.Length;
						}
					}
				}
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
			_logger?.Verbose("Extracting legacy on-premise response.");

			//using (var reader = new LegacyResponseReader(requestStream, encoding, _postDataTemporaryStore, _logger))
			//{
			//	var serializer = new JsonSerializer();
			//	var response = serializer.Deserialize(reader, typeof(OnPremiseConnectorResponse)) as OnPremiseConnectorResponse;
			//	response.Body = null;
			//	response.ContentLength = _postDataTemporaryStore.GetResponseStreamLength(reader.TemporaryId);

			//	_postDataTemporaryStore.RenameResponseStream(reader.TemporaryId, response.RequestId);

			//	_logger?.Verbose("Extracted legacy on-premise response. request-id={RequestId}, response={@Response}", response.RequestId, response);

			//	return response;
			//}

			using (var stream = new LegacyResponseStream(requestStream, _postDataTemporaryStore, _logger))
			{
				var temporaryId = await stream.ExtractBodyAsync().ConfigureAwait(false);

				using (var reader = new JsonTextReader(new StreamReader(stream)))
				{
					var message = await JToken.ReadFromAsync(reader).ConfigureAwait(false);
					var response = message.ToObject<OnPremiseConnectorResponse>();

					response.Body = null;
					response.ContentLength = await stream.DecodeBase64Response(temporaryId, response.RequestId).ConfigureAwait(false);

					_logger?.Verbose("Extracted legacy on-premise response. request-id={RequestId}, response={@Response}", response.RequestId, response);

					return response;
				}
			}
		}
	}
}
