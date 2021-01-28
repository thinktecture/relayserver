using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class RequestCoordinator<TRequest, TResponse> : IRequestCoordinator<TRequest>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RequestCoordinator<TRequest, TResponse>> _logger;
		private readonly ITenantDispatcher<TRequest> _tenantDispatcher;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;
		private readonly RelayServerOptions _relayServerOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestCoordinator{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		public RequestCoordinator(ILogger<RequestCoordinator<TRequest, TResponse>> logger, IOptions<RelayServerOptions> relayServerOptions,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry, ITenantDispatcher<TRequest> tenantDispatcher)
		{
			if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));

			_relayServerOptions = relayServerOptions.Value;
		}

		/// <inheritdoc />
		public async Task DeliverRequestAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (!_relayServerOptions.EnableRequestShortcut ||
				!await _tenantConnectorAdapterRegistry.TryDeliverRequestAsync(request, cancellationToken))
			{
				_logger.LogDebug("Redirecting request {RequestId} to dispatcher for tenant {TenantId}", request.RequestId, request.TenantId);
				await _tenantDispatcher.DispatchRequestAsync(request);
			}
		}
	}
}
