using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Diagnostics
{
	/// <summary>
	/// Represents a write for trace file data.
	/// </summary>
	public interface ITraceFileWriter
	{
		/// <summary>
		/// Writes the http headers for a traced request.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		/// <param name="headers">The http headers to write to the file.</param>
		/// <returns>An awaitable task.</returns>
		Task WriteHeaderFileAsync(string fileName, IReadOnlyDictionary<string, string> headers);

		/// <summary>
		/// Writes the body contents for a traced request.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		/// <param name="content">The http body to write to the file.</param>
		/// <returns>An awaitable task.</returns>
		Task WriteContentFileAsync(string fileName, byte[] content);
	}
}
