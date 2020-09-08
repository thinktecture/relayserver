using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class RequestCoordinator<TRequest, TResponse> : IRequestCoordinator<TRequest>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RequestCoordinator<TRequest, TResponse>> _logger;
		private readonly Guid _originId;
		private readonly ITenantDispatcher<TRequest> _tenantDispatcher;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;

		/// <summary>
		/// Initializes a new instance of <see cref="RequestCoordinator{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		public RequestCoordinator(ILogger<RequestCoordinator<TRequest, TResponse>> logger, RelayServerContext relayServerContext,
			ITenantDispatcher<TRequest> tenantDispatcher, TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_originId = relayServerContext?.OriginId ?? throw new ArgumentNullException(nameof(relayServerContext));
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));
		}

		/// <inheritdoc />
		public async Task DeliverRequestAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (!await _tenantConnectorAdapterRegistry.TryDeliverRequestAsync(request, cancellationToken))
			{
				_logger.LogDebug("Redirecting request {RequestId} to dispatcher for tenant {TenantId}", request.RequestId, request.TenantId);
				await _tenantDispatcher.DispatchRequestAsync(request);
			}
		}
	}
}
