using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	internal class RelayConnectorBuilder<TRequest, TResponse, TAcknowledge> : IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		public IServiceCollection Services { get; }

		public RelayConnectorBuilder(IServiceCollection services) => Services = services ?? throw new ArgumentNullException(nameof(services));
	}
}
