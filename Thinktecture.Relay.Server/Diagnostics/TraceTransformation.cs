using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public class TraceTransformation : ITraceTransformation
	{
		public HttpResponseMessage CreateFromTraceFile(TraceFile traceFile)
		{
			var result = new HttpResponseMessage();

			CreateContent(traceFile, result);

			return result;
		}

		private void CreateContent(TraceFile traceFile, HttpResponseMessage result)
		{
			var content = UncompressContentIfNeeded(traceFile);

			result.Content = new ByteArrayContent(content);
			result.Content.Headers.ContentLength = content.Length;
			result.Content.Headers.TryAddWithoutValidation("Content-Type", traceFile.Headers.ContainsKey("Content-Type")
				? traceFile.Headers["Content-Type"]
				: String.Empty);
		}

		private byte[] UncompressContentIfNeeded(TraceFile traceFile)
		{
			if (traceFile.Content == null)
			{
				return Array.Empty<byte>();
			}

			var contentEncoding = traceFile.Headers.ContainsKey("content-encoding") ? traceFile.Headers["content-encoding"] : String.Empty;

			if (contentEncoding == "deflate")
			{
				return UncompressDeflate(traceFile.Content);
			}

			if (contentEncoding == "gzip")
			{
				return UncompressGZip(traceFile.Content);
			}

			return traceFile.Content;
		}

		private byte[] UncompressDeflate(byte[] content)
		{
			using (var memoryStream = new MemoryStream(content))
			using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
			using (var resultStream = new MemoryStream())
			{
				deflateStream.CopyTo(resultStream, 1024);

				return resultStream.ToArray();
			}
		}

		private byte[] UncompressGZip(byte[] content)
		{
			using (var memoryStream = new MemoryStream(content))
			using (var deflateStream = new GZipStream(memoryStream, CompressionMode.Decompress))
			using (var resultStream = new MemoryStream())
			{
				deflateStream.CopyTo(resultStream, 1024);

				return resultStream.ToArray();
			}
		}
	}
}
