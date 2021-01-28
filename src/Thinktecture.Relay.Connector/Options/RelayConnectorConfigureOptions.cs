using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Targets;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorConfigureOptions : IConfigureOptions<RelayConnectorOptions>
	{
		public void Configure(RelayConnectorOptions options)
		{
			if (options.Targets.Count == 0) return;

			foreach (var (_, value) in options.Targets)
			{
				if (value.TryGetValue(Constants.RelayConnectorOptionsTargetType, out var type) && !type.Contains("."))
				{
					value[Constants.RelayConnectorOptionsTargetType] = $"{typeof(RelayWebTarget).Namespace}.{type}";
				}
			}
		}
	}
}
