using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	internal class InterceptedRequest : IInterceptedRequest
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		public Stream Stream { get; set; }
		public AcknowledgmentMode AcknowledgmentMode { get; set; }
		public Guid AcknowledgeOriginId { get; set; }
		public string ConnectionId { get; set; }

		public InterceptedRequest(IOnPremiseTargetRequest request)
		{
			RequestId = request.RequestId;
			OriginId = request.OriginId;
			AcknowledgeId = request.AcknowledgeId;
			HttpMethod = request.HttpMethod;
			Url = request.Url;
			HttpHeaders = request.HttpHeaders;
			Stream = request.Stream;
			AcknowledgmentMode = request.AcknowledgmentMode;
			AcknowledgeOriginId = request.AcknowledgeOriginId;
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}
