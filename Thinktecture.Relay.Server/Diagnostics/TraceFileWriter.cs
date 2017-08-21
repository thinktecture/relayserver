using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public class TraceFileWriter : ITraceFileWriter
	{
		public Task WriteHeaderFile(string fileName, IDictionary<string, string> headers)
		{
			var json = JsonConvert.SerializeObject(headers);
			return WriteFile(fileName, Encoding.UTF8.GetBytes(json));
		}

		public Task WriteContentFile(string fileName, byte[] content)
		{
			if (content == null)
			{
				content = new byte[0];
			}

			return WriteFile(fileName, content);
		}

		internal Task WriteFile(string fileName, byte[] content)
		{
			return Task.Factory.StartNew(() => File.WriteAllBytes(fileName, content));
		}
	}
}
