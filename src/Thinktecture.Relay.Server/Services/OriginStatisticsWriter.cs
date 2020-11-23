using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services
{
	/// <summary>
	/// An implementation that writes server startup, shutdown to
	/// </summary>
	public class OriginStatisticsWriter : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly RelayServerContext _serverContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="OriginStatisticsWriter"/>.
		/// </summary>
		/// <param name="serviceProvider">The service provider to access services from.</param>
		/// <param name="serverContext">An instance of an <see cref="RelayServerContext"/>.</param>
		public OriginStatisticsWriter(IServiceProvider serviceProvider, RelayServerContext serverContext)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_serverContext = serverContext ?? throw new ArgumentNullException(nameof(serverContext));
		}

		/// <inheritdoc />
		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetService<IStatisticsRepository>();
			if (repo != null)
			{
				await repo.CreateOriginAsync(_serverContext.OriginId);
			}

			await base.StartAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					using var scope = _serviceProvider.CreateScope();
					var sp = scope.ServiceProvider;

					var repo = sp.GetService<IStatisticsRepository>();
					if (repo != null)
					{
						await repo.HeartbeatOriginAsync(_serverContext.OriginId);

						// TODO: Use configuration for timespan to keep entries for
						await repo.CleanUpOriginsAsync(TimeSpan.FromMinutes(15));
					}

					// TODO: Use configuration for cleanup
					await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
				}
			}
			catch (TaskCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
		}

		/// <inheritdoc />
		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetService<IStatisticsRepository>();
			if (repo != null)
			{
				await repo.ShutdownOriginAsync(_serverContext.OriginId);
			}

			await base.StopAsync(cancellationToken);
		}
	}
}
