using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Persistence.Models;

/// <summary>
/// Represents a tenant config.
/// </summary>
public class Config : ITenantConfig
{
	/// <summary>
	/// The unique id of the tenant.
	/// </summary>
	public Guid TenantId { get; set; }

	/// <inheritdoc />
	public TimeSpan? KeepAliveInterval { get; set; }

	/// <inheritdoc />
	public bool? EnableTracing { get; set; }

	/// <inheritdoc />
	public TimeSpan? ReconnectMinimumDelay { get; set; }

	/// <inheritdoc />
	public TimeSpan? ReconnectMaximumDelay { get; set; }
}
