using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorRequest : IInterceptedRequest
	{
		new DateTime RequestStarted { get; set; }
		new DateTime RequestFinished { get; set; }
		new IPAddress ClientIpAddress { get; set; }
	}
}
