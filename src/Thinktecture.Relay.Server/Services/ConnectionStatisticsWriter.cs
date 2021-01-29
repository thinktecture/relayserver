using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services
{
	/// <inheritdoc />
	public class ConnectionStatisticsWriter : IConnectionStatisticsWriter
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionStatisticsWriter"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public ConnectionStatisticsWriter(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		/// <inheritdoc />
		public async Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress? remoteIpAddress,
			CancellationToken cancellationToken = default)
		{
			using var scope = _serviceProvider.CreateScope();
			await scope.ServiceProvider.GetRequiredService<IStatisticsRepository>()
				.SetConnectionTimeAsync(connectionId, tenantId, originId, remoteIpAddress, cancellationToken);
		}

		/// <inheritdoc />
		public async Task UpdateLastActivityTimeAsync(string connectionId, CancellationToken cancellationToken = default)
		{
			using var scope = _serviceProvider.CreateScope();
			await scope.ServiceProvider.GetRequiredService<IStatisticsRepository>()
				.UpdateLastActivityTimeAsync(connectionId, cancellationToken);
		}

		/// <inheritdoc />
		public async Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default)
		{
			using var scope = _serviceProvider.CreateScope();
			await scope.ServiceProvider.GetRequiredService<IStatisticsRepository>().SetDisconnectTimeAsync(connectionId, cancellationToken);
		}
	}
}
