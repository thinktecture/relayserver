using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Maintenance;

public partial class MaintenanceJobRunner
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.MaintenanceJobRunnerRunMaintenanceJob, LogLevel.Trace,
			"Running maintenance job {MaintenanceJob}")]
		public static partial void RunMaintenanceJob(ILogger logger, string? maintenanceJob);

		[LoggerMessage(LoggingEventIds.MaintenanceJobRunnerRunMaintenanceJobFailed, LogLevel.Error,
			"An error occured while running maintenance job {MaintenanceJob}")]
		public static partial void RunMaintenanceJobFailed(ILogger logger, Exception ex, string? maintenanceJob);
	}
}
