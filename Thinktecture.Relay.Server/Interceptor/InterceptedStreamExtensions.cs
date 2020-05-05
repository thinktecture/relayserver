using System;
using System.IO;
using System.Linq;
using Serilog;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal static class InterceptedStreamExtensions
	{
		public static Stream GetContentStream(this IInterceptedStream intercepted, ILogger logger)
		{
			intercepted.Stream = CreateStream(intercepted, logger);
			return intercepted.Stream;
		}

		private static Stream CreateStream(IInterceptedStream intercepted, ILogger logger)
		{
			if (intercepted.Stream == null)
			{
				return new MemoryStream(intercepted.Body ?? Array.Empty<byte>());
			}

			if (intercepted.Stream.CanSeek)
			{
				return intercepted.Stream;
			}

			logger.Information("Interceptor accessed the content of the response. Creating a COPY of the content stream to allow multiple reads of the response stream.");

			var stream = new MemoryStream();
			intercepted.Stream.CopyTo(stream);
			stream.Position = 0;

			return stream;
		}

		public static void SetContentStream(this IInterceptedStream intercepted, Stream stream, ILogger logger)
		{
			intercepted.Body = null;
			intercepted.Stream = stream;
			intercepted.ContentLength = stream.Length;

			if (intercepted.HttpHeaders.ContainsKey("Content-Length"))
			{
				var headers = intercepted.HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				headers["Content-Length"] = intercepted.ContentLength.ToString();
				intercepted.HttpHeaders = headers;
			}
		}
	}
}
