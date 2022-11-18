using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <inheritdoc/>
public partial class RequestCoordinator<T> : IRequestCoordinator<T>
	where T : IClientRequest
{
	private readonly ILogger<RequestCoordinator<T>> _logger;
	private readonly ITenantTransport<T> _tenantTransport;

	/// <summary>
	/// Initializes a new instance of the <see cref="RequestCoordinator{T}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="tenantTransport">An <see cref="ITenantTransport{T}"/>.</param>
	public RequestCoordinator(ILogger<RequestCoordinator<T>> logger, ITenantTransport<T> tenantTransport)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_tenantTransport = tenantTransport ?? throw new ArgumentNullException(nameof(tenantTransport));
	}

	[LoggerMessage(21300, LogLevel.Debug, "Redirecting request {RelayRequestId} to transport for tenant {TenantId}")]
	partial void LogRedirect(Guid relayRequestId, Guid tenantId);

	/// <inheritdoc/>
	public async Task ProcessRequestAsync(T request, CancellationToken cancellationToken = default)
	{
		LogRedirect(request.RequestId, request.TenantId);
		await _tenantTransport.TransportAsync(request);
	}
}
