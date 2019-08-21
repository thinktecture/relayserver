using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptedResponse : OnPremiseConnectorResponse, IInterceptedResponse
	{
		private readonly ILogger _logger;

		public Stream Content
		{
			get => GetContent();

			set
			{
				Stream = value;
				Body = null;
				SetContentLength(Stream);
			}
		}

		public InterceptedResponse(ILogger logger, IOnPremiseConnectorResponse other)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			RequestId = other.RequestId;
			OriginId = other.OriginId;
			RequestStarted = other.RequestStarted;
			RequestFinished = other.RequestFinished;
			StatusCode = other.StatusCode;
			HttpHeaders = other.HttpHeaders;
			Body = other.Body; // this is because of legacy on-premise connectors (v1)
			Stream = other.Stream;
			ContentLength = other.ContentLength;
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		private Stream GetContent()
		{
			if (Stream == null && Body != null)
			{
				// This means we have a legacy-type Content, so simply assign a new stream
				Stream = new MemoryStream(Body);
			}

			if (Stream != null && Body == null)
			{
				// This means we have the loaded response body available
				_logger.Information("Interceptor accessed the content of the response. Creating a COPY of the content stream to prevent multiple reads of the actual response stream.");

				Body = new byte[ContentLength];
				Stream.Read(Body, 0, (int)ContentLength);

				// switch the actual stream to a new memory stream on the content to allow for multiple reads
				Stream = new MemoryStream(Body);
			}

			return new MemoryStream(Body);
		}

		private void SetContentLength(Stream stream)
		{
			ContentLength = stream.Length;

			if (HttpHeaders.ContainsKey("Content-Length"))
			{
				var headers = CloneHttpHeaders();
				headers["Content-Length"] = ContentLength.ToString();

				HttpHeaders = headers;
			}
		}
	}
}
