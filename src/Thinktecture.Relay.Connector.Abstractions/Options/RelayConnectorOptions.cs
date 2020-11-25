using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Connector.Options
{
	/// <summary>
	/// Options for the connector.
	/// </summary>
	public class RelayConnectorOptions
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

		/// <summary>
		/// The <see cref="DiscoveryDocument"/>.
		/// </summary>
		public DiscoveryDocument DiscoveryDocument { get; set; }

		/// <summary>
		/// The web targets.
		/// </summary>
		public Dictionary<string, RelayWebTargetOptions> WebTargets { get; set; }
	}
}
