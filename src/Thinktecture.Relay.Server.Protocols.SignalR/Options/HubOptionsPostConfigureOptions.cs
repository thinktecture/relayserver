using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR.Options
{
	internal class HubOptionsPostConfigureOptions<TRequest, TResponse, TAcknowledge>
		: IPostConfigureOptions<HubOptions<ConnectorHub<TRequest, TResponse, TAcknowledge>>>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly RelayServerOptions _relayServerOptions;

		public HubOptionsPostConfigureOptions(IOptions<RelayServerOptions> relayServerOptions)
		{
			if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

			_relayServerOptions = relayServerOptions.Value;
		}

		public void PostConfigure(string name, HubOptions<ConnectorHub<TRequest, TResponse, TAcknowledge>> options)
		{
			options.HandshakeTimeout = _relayServerOptions.HandshakeTimeout;
			options.KeepAliveInterval = _relayServerOptions.KeepAliveInterval;
			// should always be twice of keep-alive
			options.ClientTimeoutInterval = _relayServerOptions.KeepAliveInterval * 2;
		}
	}
}
