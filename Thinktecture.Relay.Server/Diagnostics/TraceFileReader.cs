using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.Diagnostics
{
    public class TraceFileReader : ITraceFileReader
    {
        public virtual async Task<IDictionary<string, string>> ReadHeaderFileAsync(string fileName)
        {
            return await Task.Run(() =>
            {
                var fileContent = File.ReadAllBytes(fileName);
                var json = Encoding.UTF8.GetString(fileContent);
                var headers = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
                return headers;
            });
        }

        public virtual async Task<byte[]> ReadContentFileAsync(string fileName)
        {
            return await Task.Run(() => File.ReadAllBytes(fileName));
        }
    }
}