using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	internal class RelayConnectorBuilder<TRequest, TResponse> : IRelayConnectorBuilder<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		public IServiceCollection Services { get; }

		public RelayConnectorBuilder(IServiceCollection services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
		}
	}
}
