using System;
using System.Collections.Generic;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector
{
	// ReSharper disable once ClassNeverInstantiated.Global
	internal sealed class RelayConnectorOptions<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		public Uri DiscoveryDocument { get; set; }

		public readonly Dictionary<string, RelayTargetRegistration<TRequest, TResponse>> Targets =
			new Dictionary<string, RelayTargetRegistration<TRequest, TResponse>>(StringComparer.InvariantCultureIgnoreCase);
	}
}
