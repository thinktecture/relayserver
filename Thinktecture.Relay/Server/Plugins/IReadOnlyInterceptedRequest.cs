using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Plugins
{
	public interface IReadOnlyInterceptedRequest : IOnPremiseTargetRequest
	{
		/// <summary>
		/// Gets the date and time this request started
		/// </summary>
		DateTime RequestStarted { get; }

		/// <summary>
		/// Gets the date and time this request was finished
		/// </summary>
		DateTime RequestFinished { get; }

		/// <summary>
		/// Gets the IP address of the client sending this request. May be an IPv4 or an IPv6 address.
		/// </summary>
		IPAddress ClientIpAddress { get; }
	}
}
