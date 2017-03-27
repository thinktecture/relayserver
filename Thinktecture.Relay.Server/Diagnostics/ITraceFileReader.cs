using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Diagnostics
{
    public interface ITraceFileReader
    {
        Task<IDictionary<string, string>> ReadHeaderFileAsync(string fileName);
        Task<byte[]> ReadContentFileAsync(string fileName);
    }
}