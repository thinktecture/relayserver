using System;

namespace Thinktecture.Relay.Server.Maintenance;

/// <summary>
/// Configuration options for the statistics system.
/// </summary>
public class StatisticsOptions
{
	/// <summary>
	/// The time span to keep stale or closed connection and origin entries in the statistics store.
	/// </summary>
	public TimeSpan EntryMaxAge { get; set; } = TimeSpan.FromMinutes(15);

	/// <summary>
	/// The time interval in which the origin's last activity will be updated.
	/// </summary>
	public TimeSpan LastActivityUpdateInterval { get; set; } = TimeSpan.FromMinutes(5);
}
