using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	internal class InterceptedResponse : IInterceptedResponse
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }

		public InterceptedResponse(IOnPremiseTargetResponse response)
		{
			RequestId = response.RequestId;
			OriginId = response.OriginId;
			RequestStarted = response.RequestStarted;
			RequestFinished = response.RequestFinished;
			StatusCode = response.StatusCode;
			HttpHeaders = response.HttpHeaders;
			Stream = response.Stream;
		}

		public Dictionary<string, string> CloneHttpHeaders()
		{
			return HttpHeaders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public void Dispose()
		{
			Stream?.Dispose();
		}
	}
}
