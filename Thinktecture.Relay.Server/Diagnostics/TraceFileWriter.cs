using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public class TraceFileWriter : ITraceFileWriter
	{
		public Task WriteHeaderFileAsync(string fileName, IReadOnlyDictionary<string, string> headers)
		{
			var json = JsonConvert.SerializeObject(headers);
			return WriteAsync(fileName, Encoding.UTF8.GetBytes(json));
		}

		public Task WriteContentFileAsync(string fileName, byte[] content)
		{
			if (content == null)
				content = Array.Empty<byte>();

			return WriteAsync(fileName, content);
		}

		internal async Task WriteAsync(string fileName, byte[] content)
		{
			if (fileName == null)
				throw new ArgumentNullException(nameof(fileName));
			if (content == null)
				throw new ArgumentNullException(nameof(content));

			using (var stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await stream.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
			}
		}
	}
}
