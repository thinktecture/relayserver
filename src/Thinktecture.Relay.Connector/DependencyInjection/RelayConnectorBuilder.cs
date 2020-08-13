using System;
using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	internal class RelayConnectorBuilder : IRelayConnectorBuilder
	{
		public static readonly string RelayTargetCatchAllId = "*";

		public IServiceCollection Services { get; }

		public RelayConnectorBuilder(IServiceCollection services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
		}
	}
}
