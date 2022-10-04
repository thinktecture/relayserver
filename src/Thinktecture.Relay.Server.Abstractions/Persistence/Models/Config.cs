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

	/// <inheritdoc/>
	public TimeSpan? KeepAliveInterval { get; set; }

	/// <inheritdoc/>
	public bool? EnableTracing { get; set; }

	/// <inheritdoc/>
	public TimeSpan? ReconnectMinimumDelay { get; set; }

	/// <inheritdoc/>
	public TimeSpan? ReconnectMaximumDelay { get; set; }

	/// <summary>
	/// Update this instance with all values from the other instance.
	/// </summary>
	/// <param name="other">The source to copy the data over from to this instance.</param>
	public void UpdateFrom(Config other)
	{
		KeepAliveInterval = other.KeepAliveInterval;
		EnableTracing = other.EnableTracing;
		ReconnectMinimumDelay = other.ReconnectMinimumDelay;
		ReconnectMaximumDelay = other.ReconnectMaximumDelay;
	}
}
