using System;
using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	internal class RelayConnectorBuilder : IRelayConnectorBuilder
	{
		private bool _relayConnectorBuilt;

		public IServiceCollection Services { get; }

		public RelayConnectorBuilder()
		{
			Services = new ServiceCollection();
		}

		public RelayConnector Build()
		{
			if (_relayConnectorBuilt)
			{
				throw new InvalidOperationException("RelayConnectorBuilder allows creation only of a single instance of RelayConnector.");
			}

			_relayConnectorBuilt = true;

			var providers = Services.BuildServiceProvider();
			return providers.GetService<RelayConnector>();
		}
	}
}
