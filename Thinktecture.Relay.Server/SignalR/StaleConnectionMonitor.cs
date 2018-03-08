using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Transports;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.SignalR
{
	public class StaleConnectionMonitor : IDisposable
	{
		private readonly ITransportHeartbeat _transportHeartbeat;
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public StaleConnectionMonitor(ITransportHeartbeat transportHeartbeat, IConfiguration configuration, ILogger logger)
		{
			_transportHeartbeat = transportHeartbeat;
			_logger = logger;
			_configuration = configuration;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public void StartStaleConnectionMonitorLoop()
		{
			var token = _cancellationTokenSource.Token;

			Task.Run(async () =>
			{
				var delay = TimeSpan.FromSeconds(_configuration.KeepAliveInterval);

				while (!token.IsCancellationRequested)
				{
					try
					{
						await DisconnectStaleConnectionsAsync();
					}
					catch (Exception ex)
					{
						_logger?.Error(ex.Message);
					}
					await Task.Delay(delay, token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
		}

		private async Task DisconnectStaleConnectionsAsync()
		{
			foreach (var connection in _transportHeartbeat.GetConnections())
			{
				_logger.Verbose($"{connection.ConnectionId} state: IsTimedOut={connection.IsTimedOut}, RequiresTimeout={connection.RequiresTimeout}, SupportsKeepAlive={connection.SupportsKeepAlive}, IsAlive={connection.IsAlive}, DisconnectThreshold={connection.DisconnectThreshold}");

				if (connection.SupportsKeepAlive && connection.IsAlive)
					continue;

				_logger.Information($"Stale connection {connection.ConnectionId} was detected.");

				try
				{
					await connection.Disconnect();
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Error forcing client disconnection. connection-id={ConnectionId}", connection.ConnectionId);
				}

				_logger.Information($"Stale connection {connection.ConnectionId} was forcefully disconnected.");
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel(false);
				_cancellationTokenSource.Dispose();
			}
		}
	}
}
