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
		private readonly int _minDelay;
		private readonly int _maxDelay;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveryDocumentRetryPolicy"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public DiscoveryDocumentRetryPolicy(ILogger<DiscoveryDocumentRetryPolicy> logger,
			IOptions<RelayConnectorOptions> relayConnectorOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			if (relayConnectorOptions == null) throw new ArgumentNullException(nameof(relayConnectorOptions));

			_minDelay = relayConnectorOptions.Value.DiscoveryDocument.ReconnectMinDelay;
			_maxDelay = relayConnectorOptions.Value.DiscoveryDocument.ReconnectMaxDelay + 1;
		}

		/// <inheritdoc />
		public TimeSpan? NextRetryDelay(RetryContext retryContext)
		{
			// try an instant reconnect first
			if (retryContext.PreviousRetryCount == 0) return TimeSpan.Zero;

			var seconds = _random.Next(_minDelay, _maxDelay);
			_logger.LogDebug("Connecting attempt {ConnectionAttempt} failed and will be tried again in {ReconnectDelay} seconds",
				retryContext.PreviousRetryCount, seconds);
			return TimeSpan.FromSeconds(seconds);
		}
	}
}
