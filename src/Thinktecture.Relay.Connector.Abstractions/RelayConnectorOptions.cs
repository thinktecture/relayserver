using System;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// The configuration object to use for the connector.
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
	}
}
