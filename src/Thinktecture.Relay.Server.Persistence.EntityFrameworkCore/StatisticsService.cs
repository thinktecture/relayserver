using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <inheritdoc/>
public partial class StatisticsService : IStatisticsService
{
	private readonly RelayDbContext _dbContext;
	private readonly ILogger<StatisticsService> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatisticsService"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	public StatisticsService(ILogger<StatisticsService> logger, RelayDbContext dbContext)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	/// <inheritdoc/>
	public async Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		_logger.LogDebug(23300, "Adding new origin {OriginId} to statistics tracking", originId);

		var startup = DateTimeOffset.UtcNow;
		try
		{
			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			_dbContext.Origins.Add(new Origin()
			{
				Id = originId,
				StartupTime = startup,
				LastSeenTime = startup,
			});
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23301, ex, "An error occured while creating origin {OriginId} for statistics tracking",
				originId);
		}
	}

	[LoggerMessage(23302, LogLevel.Debug, "Updating last seen time of origin {OriginId} in statistics tracking")]
	partial void LogUpdateLastSeen(Guid originId);

	/// <inheritdoc/>
	public async Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		LogUpdateLastSeen(originId);

		try
		{
			var entity = new Origin() { Id = originId };
			_dbContext.Attach(entity);
			entity.LastSeenTime = DateTimeOffset.UtcNow;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23303, ex, "An error occured while updating origin {OriginId} for statistics tracking",
				originId);
		}
	}

	/// <inheritdoc/>
	public async Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		_logger.LogDebug(23304, "Setting shutdown time of origin {OriginId} in statistics tracking", originId);

		try
		{
			var entity = new Origin() { Id = originId };
			_dbContext.Attach(entity);
			entity.ShutdownTime = entity.LastSeenTime = DateTimeOffset.UtcNow;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23305, ex, "An error occured while updating origin {OriginId} for statistics tracking",
				originId);
		}
	}

	[LoggerMessage(23306, LogLevel.Debug,
		"Cleaning up statistics storage by deleting all origins that have not been seen since {OriginLastSeen}")]
	partial void LogCleanup(DateTimeOffset originLastSeen);

	/// <inheritdoc/>
	public async Task CleanUpOriginsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
	{
		var lastSeen = DateTimeOffset.UtcNow - maxAge;
		LogCleanup(lastSeen);

		try
		{
			var origins = await _dbContext.Origins.Where(o => o.LastSeenTime < lastSeen).ToArrayAsync(cancellationToken);
			_dbContext.Origins.RemoveRange(origins);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23307, ex, "An error occured while deleting old origins");
		}
	}

	/// <inheritdoc/>
	public async Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId,
		IPAddress? remoteIpAddress,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug(23308, "Adding new connection {TransportConnectionId} for statistics tracking",
			connectionId);

		try
		{
			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			_dbContext.Connections.Add(new Connection()
			{
				Id = connectionId,
				ConnectTime = DateTimeOffset.UtcNow,
				TenantId = tenantId,
				OriginId = originId,
				RemoteIpAddress = remoteIpAddress?.ToString(),
			});
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23309, ex,
				"An error occured while creating connection {TransportConnectionId} for statistics tracking",
				connectionId);
		}
	}

	[LoggerMessage(23310, LogLevel.Debug,
		"Updating last activity time of connection {TransportConnectionId} in statistics tracking")]
	partial void LogUpdateConnectionActivity(string transportConnectionId);

	/// <inheritdoc/>
	public async Task UpdateLastActivityTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		LogUpdateConnectionActivity(connectionId);

		try
		{
			var entity = new Connection() { Id = connectionId };
			_dbContext.Attach(entity);
			entity.LastActivityTime = DateTimeOffset.UtcNow;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23311, ex,
				"An error occured while updating connection {TransportConnectionId} in statistics tracking",
				connectionId);
		}
	}

	/// <inheritdoc/>
	public async Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		_logger.LogDebug(23312,
			"Setting disconnect time of connection {TransportConnectionId} in statistics tracking",
			connectionId);

		try
		{
			var entity = new Connection() { Id = connectionId };
			_dbContext.Attach(entity);
			entity.DisconnectTime = DateTimeOffset.UtcNow;
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23313, ex,
				"An error occured while updating connection {TransportConnectionId} in statistics tracking",
				connectionId);
		}
	}

	[LoggerMessage(23314, LogLevel.Debug,
		"Cleaning up statistics storage by deleting all connections that have no activity since {ConnectionLastActivity}")]
	partial void LogConnectionCleanup(DateTimeOffset connectionLastActivity);

	/// <inheritdoc/>
	public async Task CleanUpConnectionsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
	{
		var lastActivity = DateTimeOffset.UtcNow - maxAge;
		LogConnectionCleanup(lastActivity);

		try
		{
			var connections = await _dbContext.Connections.Where(c => c.LastActivityTime < lastActivity)
				.ToArrayAsync(cancellationToken);
			_dbContext.Connections.RemoveRange(connections);
			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			_logger.LogError(23315, ex, "An error occured while deleting old connections");
		}
	}
}
