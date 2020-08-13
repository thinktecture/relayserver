using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Protocols;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Options
{
	/// <summary>
	/// The configuration object to use for the connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class RelayConnectorOptions<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// The base uri of the server.
		/// </summary>
		public Uri RelayServerBaseUri { get; set; }

		/// <summary>
		/// The tenant name to use for authentication.
		/// </summary>
		public string TenantName { get; set; }

		/// <summary>
		/// The tenant secret to use for authentication.
		/// </summary>
		public string TenantSecret { get; set; }

		internal IConfigurationManager<DiscoveryDocument> ConfigurationManager { get; set; }

		internal readonly Dictionary<string, RelayTargetRegistration<TRequest, TResponse>> Targets
			= new Dictionary<string, RelayTargetRegistration<TRequest, TResponse>>(StringComparer.InvariantCultureIgnoreCase);
	}
}
