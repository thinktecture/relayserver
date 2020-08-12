using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay
{
	/// <summary>
	/// Extension methods for streams.
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to a <see cref="MemoryStream"/>, using a cancellation token.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> from which the contents will be copied.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="MemoryStream"/>.</returns>
		public static async Task<MemoryStream> CopyToMemoryStreamAsync(this Stream stream, CancellationToken cancellationToken = default)
		{
			var memoryStream = new MemoryStream();

			if (stream.CanSeek)
			{
				stream.Position = 0;
			}

			await stream.CopyToAsync(memoryStream, 80 * 1024, cancellationToken);

			memoryStream.Position = 0;

			return memoryStream;
		}
	}
}
