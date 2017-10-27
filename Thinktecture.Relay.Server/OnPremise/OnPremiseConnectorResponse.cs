using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.OnPremise
{
	internal class OnPremiseConnectorResponse : IOnPremiseConnectorResponse
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }

		public long ContentLength { get; set; }
	}
}
