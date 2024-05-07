using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <inheritdoc />
public partial class StatisticsService : IStatisticsService
{
	private readonly RelayDbContext _dbContext;
	private readonly ITenantService _tenantService;
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatisticsService"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	/// <param name="tenantService">An <see cref="ITenantService"/>.</param>
	public StatisticsService(ILogger<StatisticsService> logger, RelayDbContext dbContext, ITenantService tenantService)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
	}

	/// <inheritdoc />
	public async Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		Log.AddingNewOrigin(_logger, originId);

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
			Log.ErrorCreatingOrigin(_logger, ex, originId);
		}
	}

	/// <inheritdoc />
	public async Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		Log.UpdateLastSeen(_logger, originId);

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
			Log.ErrorUpdatingOrigin(_logger, ex, originId);
		}
	}

	/// <inheritdoc />
	public async Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		Log.SettingShutdownTime(_logger, originId);

		try
		{
			var entity = new Origin() { Id = originId, };
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
			Log.ErrorSettingShutdownTime(_logger, ex, originId);
		}
	}

	/// <inheritdoc />
	public async Task CleanUpOriginsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
	{
		var lastSeen = DateTimeOffset.UtcNow - maxAge;
		Log.Cleanup(_logger, lastSeen);

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
			Log.ErrorDeletingOrigins(_logger, ex);
		}
	}

	/// <inheritdoc />
	public async Task SetConnectionTimeAsync(string connectionId, string tenantName, Guid originId,
		IPAddress? remoteIpAddress,
		CancellationToken cancellationToken = default)
	{
		Log.AddingNewConnection(_logger, connectionId);

		try
		{
			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			_dbContext.Connections.Add(new Connection()
			{
				Id = connectionId,
				ConnectTime = DateTimeOffset.UtcNow,
				TenantName = _tenantService.NormalizeName(tenantName),
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
			Log.ErrorCreatingConnection(_logger, ex, connectionId);
		}
	}

	/// <inheritdoc />
	public async Task UpdateLastSeenTimeAsync(IDictionary<string, DateTimeOffset> data, CancellationToken cancellationToken = default)
	{
		var batchId = Guid.NewGuid();
		Log.UpdateConnectionsLastSeenTime(_logger, batchId, data.Count);

		try
		{
			foreach (var entry in data)
			{
				var connectionId = entry.Key;
				var lastSeenTime = entry.Value;

				Log.UpdateConnectionLastSeenTime(_logger, connectionId, lastSeenTime, batchId);

				var entity = new Connection() { Id = connectionId, };
				_dbContext.Attach(entity);

				entity.LastSeenTime = lastSeenTime;
			}

			await _dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			Log.ErrorUpdatingConnections(_logger, ex, batchId);
		}
	}

	/// <inheritdoc />
	public async Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		Log.SettingDisconnectTime(_logger, connectionId);

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
			Log.ErrorSettingDisconnectTime(_logger, ex, connectionId);
		}
	}

	/// <inheritdoc />
	public async Task CleanUpConnectionsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
	{
		var lastSeen = DateTimeOffset.UtcNow - maxAge;
		Log.ConnectionCleanup(_logger, lastSeen);

		try
		{
			var connections = await _dbContext.Connections
				.Where(c => c.LastSeenTime < lastSeen || c.DisconnectTime < lastSeen)
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
			Log.ErrorCleaningUpConnections(_logger, ex);
		}
	}
}
