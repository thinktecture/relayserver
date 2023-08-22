using System;

namespace Thinktecture.Relay.Server;

/// <summary>
/// Configuration options for the statistics system.
/// </summary>
public class StatisticsOptions
{
	/// <summary>
	/// The default entry max age.
	/// </summary>
	public static readonly TimeSpan DefaultEntryMaxAge = TimeSpan.FromMinutes(15);

	/// <summary>
	/// The default origin last seen update interval.
	/// </summary>
	public static readonly TimeSpan DefaultOriginLastSeenUpdateInterval = TimeSpan.FromMinutes(5);

	/// <summary>
	/// The default connection last seen update interval.
	/// </summary>
	public static readonly TimeSpan DefaultConnectionLastSeenUpdateInterval = TimeSpan.FromMinutes(1);

	/// <summary>
	/// The time span to keep stale or closed connection and origin entries in the statistics store.
	/// </summary>
	public TimeSpan EntryMaxAge { get; set; } = DefaultEntryMaxAge;

	/// <summary>
	/// The time interval in which the origin's last seen will be updated.
	/// </summary>
	public TimeSpan OriginLastSeenUpdateInterval { get; set; } = DefaultOriginLastSeenUpdateInterval;

	/// <summary>
	/// Indicates whether to clean up stale or closed connections or not.
	/// </summary>
	public bool EnableConnectionCleanup { get; set; }

	/// <summary>
	/// The time interval in which the connection's last seen time will be updated.
	/// </summary>
	public TimeSpan ConnectionLastSeenUpdateInterval { get; set; } = DefaultConnectionLastSeenUpdateInterval;
}
