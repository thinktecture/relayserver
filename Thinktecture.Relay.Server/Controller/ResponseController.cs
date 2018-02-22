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
				response = ForwardLegacyResponse(Encoding.UTF8, requestStream);
			}

			_logger?.Verbose("Received on-premise response. request-id={RequestId}, content-length={ResponseContentLength}", response.RequestId, response.ContentLength);

			await _backendCommunication.SendOnPremiseTargetResponseAsync(response.OriginId, response).ConfigureAwait(false);

			return Ok();
		}

		private OnPremiseConnectorResponse ForwardLegacyResponse(Encoding encoding, Stream requestStream)
		{
			_logger?.Verbose("Extracting legacy on-premise response.");

			using (var reader = new LegacyResponseReader(requestStream, encoding, _postDataTemporaryStore, _logger))
			{
				var serializer = new JsonSerializer();
				var response = serializer.Deserialize(reader, typeof(OnPremiseConnectorResponse)) as OnPremiseConnectorResponse;
				response.Body = null;
				response.ContentLength = _postDataTemporaryStore.GetResponseStreamLength(reader.TemporaryId);

				_postDataTemporaryStore.RenameResponseStream(reader.TemporaryId, response.RequestId);

				_logger?.Verbose("Extracted legacy on-premise response. request-id={RequestId}, response={@Response}", response.RequestId, response);

				return response;
			}
		}
	}
}
