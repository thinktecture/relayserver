using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

internal class InMemoryTenantTransport<T> : ITenantTransport<T>
	where T : IClientRequest
{
	private readonly ConnectorRegistry<T> _connectorRegistry;
	private readonly ILogger<InMemoryTenantTransport<T>> _logger;

	public int? BinarySizeThreshold { get; } = null;

	public InMemoryTenantTransport(ILogger<InMemoryTenantTransport<T>> logger, ConnectorRegistry<T> connectorRegistry)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
	}

	public async Task TransportAsync(T request)
	{
		if (!await _connectorRegistry.TryDeliverRequestAsync(request))
		{
			_logger.LogError(21200, "Could not deliver request {RelayRequestId} to a connection", request.RequestId);
		}
	}
}
