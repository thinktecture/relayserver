using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services
{
	/// <inheritdoc />
	public class OriginStatisticsWriter : IOriginStatisticsWriter
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of an <see cref="OriginStatisticsWriter"/>.
		/// </summary>
		/// <param name="serviceProvider">The service provider to create scopes from.</param>
		public OriginStatisticsWriter(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <inheritdoc />
		public async Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetRequiredService<IStatisticsRepository>();
			await repo.SetStartupTimeAsync(originId, cancellationToken);
		}

		/// <inheritdoc />
		public async Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetRequiredService<IStatisticsRepository>();
			await repo.UpdateLastSeenTimeAsync(originId, cancellationToken);
		}

		/// <inheritdoc />
		public async Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetRequiredService<IStatisticsRepository>();
			await repo.SetShutdownTimeAsync(originId, cancellationToken);
		}
	}
}
