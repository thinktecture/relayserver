using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Controller;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.IO
{
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
}
