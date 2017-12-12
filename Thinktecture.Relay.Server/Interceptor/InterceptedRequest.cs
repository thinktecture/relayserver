using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptedRequest : OnPremiseConnectorRequest, IInterceptedRequest
	{
		[JsonIgnore]
		public IPAddress ClientIpAddress { get; set; }

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
			Stream = other.Stream;
			ContentLength = other.ContentLength;
			AlwaysSendToOnPremiseConnector = other.AlwaysSendToOnPremiseConnector;
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}
