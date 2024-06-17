using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <inheritdoc />
public class EchoTarget<TRequest, TResponse> : IRelayTargetFunc<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : ITargetResponse, new()
{
	private class CopyStream(Stream source, long length) : Stream
	{
		private long _length = Math.Max(0, length);

		public override void Flush()
			=> throw new NotImplementedException();

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position == _length) return 0;

			var read = 0;

			while (read < count && Position < _length)
			{
				var remaining = Math.Min(_length - Position, count);
				var available = source.Read(buffer, offset, (int)remaining);
				if (available == 0)
				{
					source.Position = 0;
				}

				offset += available;
				read += available;
				count -= available;

				Position += available;
			}

			return read;
		}

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> _length = value;

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => _length;

		public override long Position { get; set; }
	}

	/// <inheritdoc />
	public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
	{
		if (!request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) || request.BodyContent is null)
			return Task.FromResult(request.CreateResponse<TResponse>(HttpStatusCode.NoContent));

		var result = request.CreateResponse<TResponse>(HttpStatusCode.OK);

		result.BodyContent = int.TryParse(request.Url, out var size)
			? new CopyStream(request.BodyContent, size)
			: request.BodyContent;
		result.BodySize = result.BodyContent.Length;
		result.BodyContent.Position = 0;

		return Task.FromResult(result);
	}
}
