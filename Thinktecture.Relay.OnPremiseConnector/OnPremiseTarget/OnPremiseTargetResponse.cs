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
		public HttpStatusCode StatusCode { get; set; }
		public IDictionary<string, string> HttpHeaders { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }

		[Obsolete("This is a legacy property for v1 OnPremiseConnector")]
		public byte[] Body { get; set; }

	}
}
