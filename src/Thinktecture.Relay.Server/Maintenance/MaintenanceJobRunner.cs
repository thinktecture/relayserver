using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Server.Maintenance
{
	/// <summary>
	/// Background service that runs all registered <see cref="IMaintenanceJob"/> implementations in an interval
	/// defined by the <see cref="MaintenanceOptions"/> configuration.
	/// </summary>
	public class MaintenanceJobRunner : BackgroundService
	{
		private readonly ILogger<MaintenanceJobRunner> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly MaintenanceOptions _maintenanceOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="MaintenanceJobRunner"/> class.
		/// </summary>
		/// <param name="logger">An instance of an <see cref="ILogger{MaintenanceJobRunner}"/>.</param>
		/// <param name="serviceProvider">An instance of an <see cref="IServiceProvider"/>.</param>
		/// <param name="maintenanceOptions">An instance of an <see cref="IOptions{MaintenanceOptions}"/>.</param>
		public MaintenanceJobRunner(ILogger<MaintenanceJobRunner> logger, IServiceProvider serviceProvider,
			IOptions<MaintenanceOptions> maintenanceOptions)
		{
			if (maintenanceOptions == null) throw new ArgumentNullException(nameof(maintenanceOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_maintenanceOptions = maintenanceOptions.Value;
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await RunMaintenanceJobsAsync(stoppingToken);
					await Task.Delay(_maintenanceOptions.RunInterval, stoppingToken);
				}
			}
			catch (OperationCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
		}

		private async Task RunMaintenanceJobsAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();

			foreach (var job in scope.ServiceProvider.GetServices<IMaintenanceJob>())
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				try
				{
					_logger.LogTrace("Running maintenance job {MaintenanceJob}.", job.GetType().FullName);
					await job.DoMaintenanceAsync(cancellationToken);
				}
				catch (OperationCanceledException)
				{
					// Ignore this, as this will be thrown when the service shuts down gracefully
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "An error occured while running maintenance job {MaintenanceJob}", job.GetType().FullName);
				}
			}
		}
	}
}
