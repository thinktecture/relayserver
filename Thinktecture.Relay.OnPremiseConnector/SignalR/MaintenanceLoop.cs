using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class MaintenanceLoop : IMaintenanceLoop
	{
		private readonly ILogger _logger;
		private readonly ITokenExpiryChecker _tokenExpiryChecker;
		private readonly IHeartbeatChecker _heartbeatChecker;
		private readonly IAutomaticDisconnectChecker _automaticDisconnectChecker;
		private readonly TimeSpan _checkInterval;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly List<IRelayServerConnection> _connections;

		// Simplest and fastest possible more or less threadsafe implementation for changing elements while looping through them in another thread.
		private IRelayServerConnection[] _connectionsForLoop;

		public MaintenanceLoop(ILogger logger, ITokenExpiryChecker tokenExpiryChecker, IHeartbeatChecker heartbeatChecker, IAutomaticDisconnectChecker automaticDisconnectChecker)
		{
			_logger = logger;
			_tokenExpiryChecker = tokenExpiryChecker ?? throw new ArgumentNullException(nameof(tokenExpiryChecker));
			_heartbeatChecker = heartbeatChecker ?? throw new ArgumentNullException(nameof(heartbeatChecker));
			_automaticDisconnectChecker = automaticDisconnectChecker ?? throw new ArgumentNullException(nameof(automaticDisconnectChecker));

			_checkInterval = TimeSpan.FromSeconds(1);
			_cancellationTokenSource = new CancellationTokenSource();
			_connections = new List<IRelayServerConnection>();
			_connectionsForLoop = Array.Empty<IRelayServerConnection>();
		}

		public void RegisterConnection(IRelayServerConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			_logger?.Information("Registering connection to {RelayServer} with maintenance loop", connection.Uri);

			lock (_connections)
			{
				_connections.Add(connection);
				_connectionsForLoop = _connections.ToArray();
			}
		}

		public void UnregisterConnection(IRelayServerConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			_logger?.Information("Unregistering connection to {RelayServer} from maintenance loop", connection.Uri);

			lock (_connections)
			{
				_connections.Remove(connection);
				_connectionsForLoop = _connections.ToArray();
			}
		}

		public void StartLoop()
		{
			var token = _cancellationTokenSource.Token;

			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
					foreach(var connection in _connectionsForLoop)
					{
						if (_automaticDisconnectChecker.DisconnectIfRequired(connection))
							continue;

						await _tokenExpiryChecker.Check(connection).ConfigureAwait(false);
						_heartbeatChecker.Check(connection);
					}

					await Task.Delay(_checkInterval, token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
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
