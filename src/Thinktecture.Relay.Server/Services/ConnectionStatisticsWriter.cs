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
		/// Initializes a new instance of an <see cref="ConnectionStatisticsWriter"/>.
		/// </summary>
		/// <param name="serviceProvider">The service provider to create scopes from.</param>
		public ConnectionStatisticsWriter(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <inheritdoc />
		public async Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress remoteIpAddress, CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetRequiredService<IStatisticsRepository>();
			await repo.SetConnectionTimeAsync(connectionId, tenantId, originId, remoteIpAddress, cancellationToken);
		}

		/// <inheritdoc />
		public async Task UpdateLastActivityTimeAsync(string connectionId, CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetRequiredService<IStatisticsRepository>();
			await repo.UpdateLastActivityTimeAsync(connectionId, cancellationToken);
		}

		/// <inheritdoc />
		public async Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var sp = scope.ServiceProvider;

			var repo = sp.GetRequiredService<IStatisticsRepository>();
			await repo.SetDisconnectTimeAsync(connectionId, cancellationToken);
		}
	}
}
