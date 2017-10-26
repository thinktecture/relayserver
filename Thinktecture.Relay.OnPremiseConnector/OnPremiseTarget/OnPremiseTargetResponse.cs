using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetResponse : IOnPremiseTargetResponse
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }

		[JsonIgnore]
		public WebResponse WebResponse { get; set; }

		public void Dispose()
		{
			WebResponse?.Dispose();
		}
	}
}
