using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Options;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	/// <inheritdoc />
	public class DiscoveryDocumentRetryPolicy : IRetryPolicy
	{
		private readonly Random _random = new Random();
		private readonly ILogger<DiscoveryDocumentRetryPolicy> _logger;

		private TimeSpan _minimumDelay = DiscoveryDocument.DefaultReconnectMinimumDelay;
		private TimeSpan _maximumDelay = DiscoveryDocument.DefaultReconnectMaximumDelay;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveryDocumentRetryPolicy"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public DiscoveryDocumentRetryPolicy(ILogger<DiscoveryDocumentRetryPolicy> logger,
			IOptions<RelayConnectorOptions> relayConnectorOptions)
		{
			if (relayConnectorOptions == null) throw new ArgumentNullException(nameof(relayConnectorOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			SetReconnectDelays(relayConnectorOptions.Value.DiscoveryDocument.ReconnectMinimumDelay,
				relayConnectorOptions.Value.DiscoveryDocument.ReconnectMaximumDelay);
		}

		/// <summary>
		/// Sets the delays for reconnecting.
		/// </summary>
		/// <param name="minimumDelay">The minimum delay to wait for reconnecting.</param>
		/// <param name="maximumDelay">The maximum delay to wait for reconnecting.</param>
		public void SetReconnectDelays(TimeSpan? minimumDelay, TimeSpan? maximumDelay)
		{
			minimumDelay ??= _minimumDelay;
			maximumDelay ??= _maximumDelay;

			if (minimumDelay == _minimumDelay && maximumDelay == _maximumDelay) return;

			if (minimumDelay > maximumDelay)
			{
				_logger.LogWarning(
					"Keeping (default) reconnect delays because minimum ({ReconnectMinimumDelay}) cannot be greater than maximum ({ReconnectMaximumDelay})",
					minimumDelay, maximumDelay);
			}
			else
			{
				_logger.LogDebug("Using a minimum of {ReconnectMinimumDelay} and a maximum of {ReconnectMaximumDelay} for reconnecting",
					minimumDelay, maximumDelay);

				_minimumDelay = minimumDelay.Value;
				_maximumDelay = maximumDelay.Value;
			}
		}

		/// <inheritdoc />
		public TimeSpan? NextRetryDelay(RetryContext retryContext)
		{
			// try an instant reconnect first
			if (retryContext.PreviousRetryCount == 0) return TimeSpan.Zero;

			// add one second because it's used as an exclusive upper bound later
			var seconds = _random.Next((int)_minimumDelay.TotalSeconds, (int)_maximumDelay.TotalSeconds + 1);
			_logger.LogDebug("Connecting attempt {ConnectionAttempt} failed and will be tried again in {ReconnectDelay} seconds",
				retryContext.PreviousRetryCount, seconds);

			return TimeSpan.FromSeconds(seconds);
		}
	}
}
