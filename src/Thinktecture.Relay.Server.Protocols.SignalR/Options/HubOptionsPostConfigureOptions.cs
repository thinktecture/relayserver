using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR.Options
{
	internal class HubOptionsPostConfigureOptions<TRequest, TResponse> : IPostConfigureOptions<HubOptions<ConnectorHub<TRequest, TResponse>>>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		private readonly RelayServerOptions _relayServerOptions;

		public HubOptionsPostConfigureOptions(IOptions<RelayServerOptions> relayServerOptions)
			=> _relayServerOptions = relayServerOptions?.Value ?? throw new ArgumentNullException(nameof(relayServerOptions));

		public void PostConfigure(string name, HubOptions<ConnectorHub<TRequest, TResponse>> options)
		{
			options.HandshakeTimeout = _relayServerOptions.HandshakeTimeout;
			options.KeepAliveInterval = _relayServerOptions.KeepAliveInterval;
			// should always be twice of keep-alive
			options.ClientTimeoutInterval = _relayServerOptions.KeepAliveInterval * 2;
		}
	}
}
