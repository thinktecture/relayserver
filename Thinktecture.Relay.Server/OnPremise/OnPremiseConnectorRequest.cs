using System;
using Newtonsoft.Json;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.Server.OnPremise
{
	internal class OnPremiseConnectorRequest : OnPremiseTargetRequest, IOnPremiseConnectorRequest, IInterceptedRequest
	{
		[JsonIgnore]
		public DateTime RequestStarted { get; set; }
		[JsonIgnore]
		public DateTime RequestFinished { get; set; }
		[JsonIgnore]
		public string ClientIpAddress { get; set; }
	}
}
