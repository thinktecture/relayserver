using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using Newtonsoft.Json;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptedRequest : OnPremiseConnectorRequest, IInterceptedRequest
	{
		[JsonIgnore]
		public IPAddress ClientIpAddress { get; set; }

		[JsonIgnore]
		public IPrincipal ClientUser { get; set; }

		public InterceptedRequest(IOnPremiseConnectorRequest other)
		{
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
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}
