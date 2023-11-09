using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Server.Persistence.Models;

/// <summary>
/// Represents a tenant. A tenant can have multiple connectors on one physical on-premises.
/// </summary>
public class Tenant
{
	/// <summary>
	/// The unique name of the tenant.
	/// </summary>
	/// <remarks>The maximum length is 100 unicode characters.</remarks>
	public string Name { get; set; } = default!;

	/// <summary>
	/// The display name of the tenant. Will be used as a visual identifier on the management UI.
	/// </summary>
	/// <remarks>The maximum length is 200 unicode characters.</remarks>
	public string? DisplayName { get; set; }

	/// <summary>
	/// An optional, longer, textual description of this tenant.
	/// </summary>
	/// <remarks>The maximum length is 1000 unicode characters.</remarks>
	public string? Description { get; set; }

	/// <summary>
	/// The maximum amount of concurrent requests a connector should receive.
	/// <remarks>Defaults to 0 (unlimited).</remarks>
	/// </summary>
	public int MaximumConcurrentConnectorRequests { get; set; }

	/// <summary>
	/// Enable the requirement that only an authenticated request can use this tenant to relay requests.
	/// </summary>
	public bool RequireAuthentication { get; set; }

	/// <summary>
	/// The normalized (e.g. ToUpperInvariant()) name of the tenant. Use this for case-insensitive comparison in the database.
	/// </summary>
	/// <remarks>The maximum length is 100 unicode characters.</remarks>
	[JsonIgnore]
	public string NormalizedName { get; set; } = default!;

	/// <summary>
	/// The client secrets, used for authentication connectors for this tenant.
	/// </summary>
	[JsonIgnore]
	public List<ClientSecret>? ClientSecrets { get; set; }

	/// <summary>
	/// The connections for this tenant.
	/// </summary>
	[JsonIgnore]
	public List<Connection>? Connections { get; set; }

	/// <summary>
	/// The requests handled for this tenant.
	/// </summary>
	[JsonIgnore]
	public List<Request>? Requests { get; set; }

	/// <summary>
	/// The config for this tenant.
	/// </summary>
	[JsonIgnore]
	public Config? Config { get; set; }
}
