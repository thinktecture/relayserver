using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Newtonsoft.Json;
using Serilog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptedRequest : OnPremiseConnectorRequest, IInterceptedRequest
	{
		private readonly ILogger _logger;

		[JsonIgnore]
		public IPAddress ClientIpAddress { get; set; }

		[JsonIgnore]
		public IPrincipal ClientUser { get; set; }

		[JsonIgnore]
		public Uri ClientRequestUri { get; set; }

		[JsonIgnore]
		public Stream Content
		{
			get => GetContent();
			set
			{
				Stream = value;
				SetContentLength(Stream);
			}
		}

		public InterceptedRequest(ILogger logger, IOnPremiseConnectorRequest other)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));;

			RequestId = other.RequestId;
			OriginId = other.OriginId;
			AcknowledgeId = other.AcknowledgeId;
			RequestStarted = other.RequestStarted;
			RequestFinished = other.RequestFinished;
			HttpMethod = other.HttpMethod;
			Url = other.Url;
			HttpHeaders = other.HttpHeaders;
			Body = other.Body;
			AcknowledgmentMode = other.AcknowledgmentMode;
			Stream = other.Stream;
			ContentLength = other.ContentLength;
			AlwaysSendToOnPremiseConnector = other.AlwaysSendToOnPremiseConnector;
			Expiration = other.Expiration;
			AcknowledgeOriginId = other.AcknowledgeOriginId;
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		private Stream GetContent()
		{
			if (Body == null)
			{
				_logger.Information("Interceptor accessed the content of the request. Creating a COPY of the content stream to prevent multiple reads of the actual request stream. This might cause additional memory overhead.");

				Body = new byte[ContentLength];
				Stream.Read(Body, 0, (int)ContentLength);
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
