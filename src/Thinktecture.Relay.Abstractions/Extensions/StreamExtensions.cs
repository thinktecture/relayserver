using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace; (extension methods on Stream namespace)
namespace System.IO
{
	/// <summary>
	/// Extension methods for streams.
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		/// Sets the position of the stream to zero, if the stream supports seeking.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> that will be rewound.</param>
		/// <returns>true, if rewinding was possible; otherwise, false</returns>
		public static bool TryRewind(this Stream stream)
		{
			if (!stream.CanSeek)
			{
				return false;
			}

			stream.Position = 0;
			return true;
		}

		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to a <see cref="MemoryStream"/>, using a cancellation token.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> from which the contents will be copied.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="MemoryStream"/>.</returns>
		public static async Task<MemoryStream> CopyToMemoryStreamAsync(this Stream stream, CancellationToken cancellationToken = default)
		{
			stream.TryRewind();

			var memoryStream = new MemoryStream(stream.CanSeek ? (int)stream.Length : 1024 * 1024);
			await stream.CopyToAsync(memoryStream, 80 * 1024, cancellationToken);

			memoryStream.Position = 0;

			return memoryStream;
		}
	}
}
