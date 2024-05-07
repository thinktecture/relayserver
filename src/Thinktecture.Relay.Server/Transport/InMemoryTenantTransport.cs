using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

internal partial class InMemoryTenantTransport<T> : ITenantTransport<T>
	where T : IClientRequest
{
	private readonly ConnectorRegistry<T> _connectorRegistry;
	private readonly ILogger _logger;

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
			Log.ErrorDeliveringRequest(_logger, request.RequestId);
		}
	}
}
