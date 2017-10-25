using System;
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
			var fileContent = await ReadContentFileAsync(fileName).ConfigureAwait(false);
			var json = Encoding.UTF8.GetString(fileContent);

			return JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
		}

		public virtual async Task<byte[]> ReadContentFileAsync(string fileName)
		{
			using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var offset = 0;
				var length = (int)stream.Length;
				var buffer = new byte[length];

				while (length > 0)
				{
					var read = await stream.ReadAsync(buffer, offset, length).ConfigureAwait(false);

					if (read == 0)
						return buffer;

					offset += read;
					length -= read;
				}

				return buffer;
			}
		}
	}
}
