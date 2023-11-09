using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

/// <inheritdoc />
public partial class DiscoveryDocumentRetryPolicy : IRetryPolicy
{
	private readonly ILogger _logger;
	private readonly Random _random = new Random();

	private TimeSpan _maximumDelay = DiscoveryDocument.DefaultReconnectMaximumDelay;
	private TimeSpan _minimumDelay = DiscoveryDocument.DefaultReconnectMinimumDelay;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscoveryDocumentRetryPolicy"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
	public DiscoveryDocumentRetryPolicy(ILogger<DiscoveryDocumentRetryPolicy> logger,
		IOptions<RelayConnectorOptions> relayConnectorOptions)
	{
		if (relayConnectorOptions is null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		SetReconnectDelays(relayConnectorOptions.Value.DiscoveryDocument.ReconnectMinimumDelay,
			relayConnectorOptions.Value.DiscoveryDocument.ReconnectMaximumDelay);
	}

	/// <inheritdoc />
	public TimeSpan? NextRetryDelay(RetryContext retryContext)
	{
		// try an instant reconnect first
		if (retryContext.PreviousRetryCount == 0) return TimeSpan.Zero;

		// add one second because it's used as an exclusive upper bound later
		var seconds = _random.Next((int)_minimumDelay.TotalSeconds, (int)_maximumDelay.TotalSeconds + 1);
		Log.LogRetry(_logger, retryContext.PreviousRetryCount, seconds);

		return TimeSpan.FromSeconds(seconds);
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
			Log.KeepingDefaults(_logger, minimumDelay, maximumDelay);
		}
		else
		{
			Log.UsingDelays(_logger, minimumDelay, maximumDelay);
			_minimumDelay = minimumDelay.Value;
			_maximumDelay = maximumDelay.Value;
		}
	}
}
