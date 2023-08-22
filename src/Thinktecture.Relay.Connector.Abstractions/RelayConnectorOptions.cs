using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Thinktecture.Relay.Connector;

/// <summary>
/// Options for the connector.
/// </summary>
public class RelayConnectorOptions
{
	/// <summary>
	/// The base uri of the server.
	/// </summary>
	public Uri RelayServerBaseUri { get; set; } = default!;

	/// <summary>
	/// The tenant name to use for authentication.
	/// </summary>
	public string TenantName { get; set; } = default!;

	/// <summary>
	/// The tenant secret to use for authentication.
	/// </summary>
	public string TenantSecret { get; set; } = default!;

	/// <summary>
	/// The <see cref="DiscoveryDocument"/>.
	/// </summary>
	public DiscoveryDocument DiscoveryDocument { get; set; } = default!;

	/// <summary>
	/// Indicates that the <see cref="HttpClient"/> used to communicate with the server uses the HTTP keep-alive feature.
	/// </summary>
	public bool UseHttpKeepAlive { get; set; } = true;

	/// <summary>
	/// The targets keyed by id.
	/// </summary>
	public Dictionary<string, Dictionary<string, string>> Targets { get; set; } = default!;
}
