using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public class OnPremiseConnectionHeartbeater : IOnPremiseConnectionHeartbeater
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly IBackendCommunication _backendCommunication;

		private readonly TimeSpan _heartbeatInterval;
		private readonly CancellationTokenSource _cts;

		public OnPremiseConnectionHeartbeater(ILogger logger, IConfiguration configuration, IBackendCommunication backendCommunication)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_backendCommunication = backendCommunication ?? throw new ArgumentNullException(nameof(backendCommunication));

			_heartbeatInterval = new TimeSpan(_configuration.ActiveConnectionTimeout.Ticks / 4);
			_cts = new CancellationTokenSource();
		}

		public void Prepare()
		{
			StartSendHeartbeatsLoop(_cts.Token);
		}

		private void StartSendHeartbeatsLoop(CancellationToken token)
		{
			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
#pragma warning disable CS4014 // This should be started but not awaited
					Task.WhenAll(_backendCommunication.GetConnectionContexts().Select(async connectionContext =>
					{
						await MarkConnectionInactiveIfTimedOut(connectionContext).ConfigureAwait(false);
						return SendHeartbeatAsync(connectionContext, token);
					})).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
		}

		private async Task SendHeartbeatAsync(IOnPremiseConnectionContext connectionContext, CancellationToken token)
		{
			if (connectionContext == null)
				throw new ArgumentNullException(nameof(connectionContext));

			if (connectionContext.NextHeartbeat > DateTime.UtcNow)
			{
				return;
			}

			connectionContext.NextHeartbeat = DateTime.UtcNow.Add(_heartbeatInterval);

			try
			{
				_logger?.Verbose("Sending {PingMethod}. connection-id={ConnectionId}", connectionContext.SupportsHeartbeat ? "Heartbeat" : "Ping (as heartbeat)", connectionContext.ConnectionId);

				var requestId = Guid.NewGuid().ToString();
				var request = new OnPremiseConnectorRequest()
				{
					HttpMethod = connectionContext.SupportsHeartbeat ? "HEARTBEAT" : "PING",
					Url = String.Empty,
					RequestStarted = DateTime.UtcNow,
					OriginId = _backendCommunication.OriginId,
					RequestId = requestId,
					AcknowledgmentMode = AcknowledgmentMode.Auto,
					HttpHeaders = connectionContext.SupportsConfiguration ? null : new Dictionary<string, string> { ["X-TTRELAY-HEARTBEATINTERVAL"] = _heartbeatInterval.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
				};

				// wait for the response of the Heartbeat / Ping
				var task = _backendCommunication.GetResponseAsync(requestId, _configuration.ActiveConnectionTimeout);

				// heartbeats do NOT go through the message dispatcher as we want to heartbeat the connections directly
				await connectionContext.RequestAction(request, token).ConfigureAwait(false);
				var response = await task.ConfigureAwait(false);

				if (response != null)
				{
					await _backendCommunication.RenewLastActivityAsync(connectionContext.ConnectionId).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during sending heartbeat to a client. link-id={LinkId}, connection-id={ConnectionId}, connector-version={ConnectorVersion}", connectionContext.LinkId, connectionContext.ConnectionId, connectionContext.ConnectorVersion);
			}
		}

		private async Task MarkConnectionInactiveIfTimedOut(IOnPremiseConnectionContext connectionContext)
		{
			if (connectionContext.IsActive && connectionContext.LastLocalActivity + _configuration.ActiveConnectionTimeout < DateTime.UtcNow)
			{
				await _backendCommunication.DeactivateOnPremiseConnectionAsync(connectionContext.ConnectionId);
			}
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cts.Cancel();
				_cts.Dispose();
			}
		}

		#endregion
	}
}
