using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public interface ITraceFileWriter
	{
		Task WriteHeaderFile(string fileName, IDictionary<string, string> headers);
		Task WriteContentFile(string fileName, byte[] content);
	}
}
