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

		[Obsolete("Will only be used by legacy on premise connectors (v1)")]
		public byte[] Body { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }

		[JsonIgnore]
		public long ContentLength { get; set; }
	}
}
