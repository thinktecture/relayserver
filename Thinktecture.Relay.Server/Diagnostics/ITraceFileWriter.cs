using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public interface ITraceFileWriter
	{
		Task WriteHeaderFileAsync(string fileName, IReadOnlyDictionary<string, string> headers);
		Task WriteContentFileAsync(string fileName, byte[] content);
	}
}
