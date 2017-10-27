using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.OnPremise
{
	internal class OnPremiseConnectorRequest : OnPremiseTargetRequest, IOnPremiseConnectorRequest
	{
		[JsonIgnore]
		public DateTime RequestStarted { get; set; }
		[JsonIgnore]
		public DateTime RequestFinished { get; set; }
		[JsonIgnore]
		public IPAddress ClientIpAddress { get; set; }

		IDictionary<string, string> IWriteableOnPremiseTargetRequest.HttpHeaders => HttpHeaders;
	}
}
