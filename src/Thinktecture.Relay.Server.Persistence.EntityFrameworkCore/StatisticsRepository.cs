using System;
using System.Linq;
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
		public async Task CreateOriginAsync(Guid originId)
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
		public async Task HeartbeatOriginAsync(Guid originId)
		{
			_logger.LogDebug("Updating heartbeat time of origin with id {OriginId} in statistics tracking.", originId);

			var entity = await GetOrCreateOriginEntityAsync(originId);
			entity.HeartbeatTime = DateTime.UtcNow;

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
		public async Task ShutdownOriginAsync(Guid originId)
		{
			_logger.LogDebug("Marking origin with id {OriginId} as stopped in statistics tracking.", originId);

			var now = DateTime.UtcNow;

			var entity = await GetOrCreateOriginEntityAsync(originId);
			entity.HeartbeatTime = now;
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
		public async Task CleanUpOriginsAsync(TimeSpan oldestToKeep)
		{
			var filterTime = DateTime.UtcNow - oldestToKeep;

			_logger.LogDebug("Cleaning up statistics storage: Deleting all origins that have not been updates since {OldestOriginToKeep}.", filterTime);

			var entitiesToDelete = await _dbContext.Origins
				.Where(o => o.HeartbeatTime < filterTime)
				.ToArrayAsync();

			_dbContext.Origins.RemoveRange(entitiesToDelete);

			try
			{
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while cleaning up old origins.");
			}
		}

		private async Task<Origin> CreateOriginInternalAsync(Guid originId)
		{
			var now = DateTime.UtcNow;

			var entity = new Origin()
			{
				Id = originId,
				StartTime = now,
				HeartbeatTime = now,
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
