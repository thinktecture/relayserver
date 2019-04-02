using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Diagnostics
{
	/// <summary>
	/// Represents a reader for trace file data.
	/// </summary>
	public interface ITraceFileReader
	{
		/// <summary>
		/// Reads the headers of the trace file.
		/// </summary>
		/// <param name="fileName">The file name to read.</param>
		/// <returns>The http headers of the traced request.</returns>
		Task<IDictionary<string, string>> ReadHeaderFileAsync(string fileName);

		/// <summary>
		/// Reads the contents of the trace file.
		/// </summary>
		/// <param name="fileName">The file name to read.</param>
		/// <returns>The http body of the traced request.</returns>
		Task<byte[]> ReadContentFileAsync(string fileName);
	}
}
