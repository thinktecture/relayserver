using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <inheritdoc />
	public class StatisticsRepository : IStatisticsRepository
	{
		private readonly ILogger<StatisticsRepository> _logger;
		private readonly RelayDbContext _dbContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="StatisticsRepository"/> class.
		/// </summary>
		/// <param name="logger">The logger to log to.</param>
		/// <param name="dbContext">The Entity Framework Core database context.</param>
		public StatisticsRepository(ILogger<StatisticsRepository> logger, RelayDbContext dbContext)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		/// <inheritdoc />
		public async Task SetStartupTimeAsync(Guid originId)
		{
			_logger.LogDebug("Adding a new origin with id {OriginId} to statistics tracking.", originId);
			await CreateOriginInternalAsync(originId);

			try
			{
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while saving origin with id {OriginId} to statistics database.", originId);
			}
		}

		/// <inheritdoc />
		public async Task UpdateLastSeenTimeAsync(Guid originId)
		{
			_logger.LogDebug("Updating heartbeat time of origin with id {OriginId} in statistics tracking.", originId);

			var entity = await GetOrCreateOriginEntityAsync(originId);
			entity.LastSeenTime = DateTime.UtcNow;

			try
			{
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while updating origin with id {OriginId} in statistics database.", originId);
			}
		}

		/// <inheritdoc />
		public async Task SetShutdownTimeAsync(Guid originId)
		{
			_logger.LogDebug("Marking origin with id {OriginId} as stopped in statistics tracking.", originId);

			var now = DateTime.UtcNow;

			var entity = await GetOrCreateOriginEntityAsync(originId);
			entity.LastSeenTime = now;
			entity.ShutdownTime = now;

			try
			{
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while updating origin with id {OriginId} in statistics database.", originId);
			}
		}

		/// <inheritdoc />
		public async Task CleanUpOriginsAsync(TimeSpan maxAge)
		{
			var filterTime = DateTime.UtcNow - maxAge;

			_logger.LogDebug("Cleaning up statistics storage: Deleting all origins that have not been updated since {OldestOriginToKeep}.", filterTime);

			try
			{
				var originsToDelete = await _dbContext.Origins
					.Where(o => o.LastSeenTime < filterTime)
					.ToArrayAsync();

				_dbContext.Origins.RemoveRange(originsToDelete);
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while cleaning up old origins.");
			}
		}

		/// <inheritdoc />
		public async Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress remoteIpAddress)
		{
			_logger.LogDebug("Adding a new connection with id {ConnectionId} to statistics tracking.", connectionId);

			try
			{
				var entity = new Connection()
				{
					Id = connectionId,
					ConnectTime = DateTime.UtcNow,
					TenantId = tenantId,
					OriginId = originId,
					RemoteIpAddress = remoteIpAddress.ToString(),
				};

				await _dbContext.Connections.AddAsync(entity);
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while saving connection with id {ConnectionId} to statistics database.", originId);
			}
		}

		/// <inheritdoc />
		public async Task UpdateLastActivityTimeAsync(string connectionId)
		{
			_logger.LogDebug("Updating heartbeat time of connection with id {ConnectionId} in statistics tracking.", connectionId);

			try
			{
				var entity = await _dbContext.Connections.FirstAsync(c => c.Id == connectionId);
				entity.LastActivityTime = DateTime.UtcNow;

				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while updating origin with id {ConnectionId} in statistics database.", connectionId);
			}
		}

		/// <inheritdoc />
		public async Task SetDisconnectTimeAsync(string connectionId)
		{
			_logger.LogDebug("Marking connection with id {ConnectionId} as stopped in statistics tracking.", connectionId);

			try
			{
				var entity = await _dbContext.Connections.FirstAsync(c => c.Id == connectionId);
				entity.DisconnectTime = DateTime.UtcNow;

				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while updating connection with id {ConnectionId} in statistics database.", connectionId);
			}
		}

		/// <inheritdoc />
		public async Task CleanUpConnectionsAsync(TimeSpan maxAge)
		{
			var filterTime = DateTime.UtcNow - maxAge;

			_logger.LogDebug("Cleaning up statistics storage: Deleting all connections that have not been updated since {OldestOriginToKeep}.", filterTime);

			try
			{
				var connectionsToDelete = await _dbContext.Connections
					.Where(c => c.LastActivityTime < filterTime)
					.ToArrayAsync();

				_dbContext.Connections.RemoveRange(connectionsToDelete);
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while cleaning up old connections.");
			}
		}

		private async Task<Origin> CreateOriginInternalAsync(Guid originId)
		{
			var now = DateTime.UtcNow;

			var entity = new Origin()
			{
				Id = originId,
				StartupTime = now,
				LastSeenTime = now,
			};

			await _dbContext.Origins.AddAsync(entity);
			return entity;
		}

		private async Task<Origin> GetOrCreateOriginEntityAsync(Guid originId)
		{
			return await _dbContext.Origins.FirstOrDefaultAsync(o => o.Id == originId)
			       ?? await CreateOriginInternalAsync(originId);
		}
	}
}
