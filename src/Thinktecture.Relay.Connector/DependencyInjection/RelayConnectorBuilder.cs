using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	internal class RelayConnectorBuilder : IRelayConnectorBuilder
	{
		public IServiceCollection Services { get; }

		public RelayConnectorBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}
