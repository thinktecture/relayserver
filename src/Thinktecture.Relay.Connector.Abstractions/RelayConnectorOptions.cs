using System;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// The configuration object to use for the connector.
	/// </summary>
	public class RelayConnectorOptions
	{
		private DiscoveryDocument _discoveryDocument;

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
		public DiscoveryDocument DiscoveryDocument
		{
			get => _discoveryDocument ?? throw new InvalidOperationException("The discovery document is not yet available.");
			set => _discoveryDocument = value;
		}
	}
}
