using System;

namespace Thinktecture.Relay.Server.Maintenance;

/// <summary>
/// Configuration options for the maintenance system.
/// </summary>
public class MaintenanceOptions
{
	/// <summary>
	/// The interval in which maintenance jobs will be run.
	/// </summary>
	public TimeSpan RunInterval { get; set; } = TimeSpan.FromMinutes(15);
}
