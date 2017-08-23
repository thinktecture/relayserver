using System;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorRequest : IOnPremiseTargetRequest
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
		string ClientIpAddress { get; }
	}
}
