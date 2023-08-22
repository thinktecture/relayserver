using System;

namespace Thinktecture.Relay.Server;

/// <summary>
/// Configuration options for the maintenance system.
/// </summary>
public class MaintenanceOptions
{
	/// <summary>
	/// The default run interval.
	/// </summary>
	public static readonly TimeSpan DefaultRunInterval = TimeSpan.FromMinutes(15);

	/// <summary>
	/// The interval in which maintenance jobs will be run.
	/// </summary>
	public TimeSpan RunInterval { get; set; } = DefaultRunInterval;
}
