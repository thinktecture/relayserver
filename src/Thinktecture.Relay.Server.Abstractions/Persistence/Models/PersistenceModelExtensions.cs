using System;
using System.Collections.Generic;
using System.Linq;

namespace Thinktecture.Relay.Server.Persistence.Models;

/// <summary>
/// Extension methods for the persistence model types.
/// </summary>
public static class PersistenceModelsExtensions
{
	/// <summary>
	/// Update this instance with all values from the other instance.
	/// </summary>
	/// <param name="instance">Instance to update from the <paramref name="other"/> instance.</param>
	/// <param name="other">The source to copy the data over from to this instance.</param>
	public static void UpdateFrom(this ClientSecret instance, ClientSecret other)
	{
		if (instance.Id != other.Id)
			throw new InvalidOperationException(
				$"Id of other secret ({other.Id}) is not the same as this ({instance.Id}). Cannot update from other instance.");

		instance.Created = other.Created;
		instance.Value = other.Value;
		instance.Expiration = other.Expiration;
	}

	/// <summary>
	/// Update this instance with all values from the other instance.
	/// </summary>
	/// <param name="instance">Instance to update from the <paramref name="other"/> instance.</param>
	/// <param name="other">The source to copy the data over from to this instance.</param>
	public static void UpdateFrom(this Config instance, Config other)
	{
		instance.TenantName = other.TenantName;
		instance.KeepAliveInterval = other.KeepAliveInterval;
		instance.EnableTracing = other.EnableTracing;
		instance.ReconnectMinimumDelay = other.ReconnectMinimumDelay;
		instance.ReconnectMaximumDelay = other.ReconnectMaximumDelay;
	}

	/// <summary>
	/// If both tenants have the same id, update this instance with all values from the other instance.
	/// </summary>
	/// <param name="instance">Instance to update from the <paramref name="other"/> instance.</param>
	/// <param name="other">The source to copy the data over from to this instance.</param>
	public static void UpdateFrom(this Tenant instance, Tenant other)
	{
		if (instance.NormalizedName != other.NormalizedName)
			throw new InvalidOperationException(
				$"Name of other tenant ({other.Name}) is not the same as this ({instance.Name}). Cannot update from other instance.");

		instance.DisplayName = other.DisplayName;
		instance.Description = other.Description;
		instance.RequireAuthentication = other.RequireAuthentication;
		instance.MaximumConcurrentConnectorRequests = other.MaximumConcurrentConnectorRequests;

		// copy over config if available
		if (other.Config is not null)
		{
			instance.Config ??= new Config();
			instance.Config.UpdateFrom(other.Config);
		}
	}
}
