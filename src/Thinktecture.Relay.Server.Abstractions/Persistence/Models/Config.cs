using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Persistence.Models;

/// <summary>
/// Represents a tenant config.
/// </summary>
public class Config : ITenantConfig
{
	/// <summary>
	/// The unique name of the tenant.
	/// </summary>
	public string TenantName { get; set; } = default!;

	/// <inheritdoc />
	public TimeSpan? KeepAliveInterval { get; set; }

	/// <inheritdoc />
	public bool? EnableTracing { get; set; }

	/// <inheritdoc />
	public TimeSpan? ReconnectMinimumDelay { get; set; }

	/// <inheritdoc />
	public TimeSpan? ReconnectMaximumDelay { get; set; }
}
