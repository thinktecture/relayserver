using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Targets;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorConfigureOptions : IConfigureOptions<RelayConnectorOptions>
	{
		public void Configure(RelayConnectorOptions options)
		{
			if (options.Targets == null || options.Targets.Count == 0) return;

			foreach (var kvp in options.Targets)
			{
				if (kvp.Value.TryGetValue(Constants.RelayConnectorOptionsTargetType, out var type) && !type.Contains("."))
				{
					kvp.Value[Constants.RelayConnectorOptionsTargetType] = $"{typeof(RelayWebTarget).Namespace}.{type}";
				}
			}
		}
	}
}
