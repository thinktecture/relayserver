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
	internal class InterceptedRequest : OnPremiseConnectorRequest, IInterceptedRequest, IInterceptedStream
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
			get => this.GetContentStream(_logger);
			set => this.SetContentStream(value, _logger);
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
	}
}
