using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorRequest : IInterceptedRequest, IOnPremiseTargetRequest
	{
		new DateTime RequestStarted { get; set; }
		new DateTime RequestFinished { get; set; }
		new string ClientIpAddress { get; set; }
	}
}
