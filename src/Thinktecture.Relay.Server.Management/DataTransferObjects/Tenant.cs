using System;
using System.ComponentModel.DataAnnotations;

namespace Thinktecture.Relay.Server.Management.DataTransferObjects;

/// <summary>
/// Represents a tenant. A tenant can have multiple connectors on one physical on-premises
/// </summary>
public class Tenant
{
	/// <summary>
	/// Gets or sets name of the tenant. Also used as ClientId for connector authentication.
	/// </summary>
	/// <example>MyOnPremiseConnector</example>
	[Required]
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the display name of the tenant. Will be used as a visual identifier on the management UI.
	/// </summary>
	/// <example>My on premise connector</example>
	public string? DisplayName { get; set; }

	/// <summary>
	/// Gets or sets an optional, longer, textual description of this tenant.
	/// </summary>
	/// <example>On premise connector in the Thinktecture office in Karlsruhe</example>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets whether authentication is required for relaying a request to this tenant.
	/// </summary>
	public bool? RequireAuthentication { get; set; }

	/// <summary>
	/// Gets or sets the maximum concurrent connector requests.
	/// </summary>
	public int? MaximumConcurrentConnectorRequests { get; set; }

	/// <summary>
	/// Gets or sets the interval used to send keep alive pings between the server and a connector.
	/// </summary>
	public TimeSpan? KeepAliveInterval { get; set; }

	/// <summary>
	/// Gets or sets whether tracing is enabled for all requests of this particular tenant.
	/// </summary>
	public bool? EnableTracing { get; set; }

	/// <summary>
	/// Gets or sets the minimum delay to wait for until a reconnect of a connector should be attempted again.
	/// </summary>
	public TimeSpan? ReconnectMinimumDelay { get; set; }

	/// <summary>
	/// Gets or sets maximum delay to wait for until a reconnect of a connector should be attempted again.
	/// </summary>
	public TimeSpan? ReconnectMaximumDelay { get; set; }
}
