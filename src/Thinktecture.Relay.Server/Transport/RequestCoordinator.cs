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
		private readonly bool _enableRequestShortcut;

		/// <summary>
		/// Initializes a new instance of <see cref="RequestCoordinator{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public RequestCoordinator(ILogger<RequestCoordinator<TRequest, TResponse>> logger, ITenantDispatcher<TRequest> tenantDispatcher,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry, IOptions<RelayServerOptions> options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));

			_enableRequestShortcut = options.Value.EnableRequestShortcut;
		}

		/// <inheritdoc />
		public async Task DeliverRequestAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (!_enableRequestShortcut || !await _tenantConnectorAdapterRegistry.TryDeliverRequestAsync(request, cancellationToken))
			{
				_logger.LogDebug("Redirecting request {RequestId} to dispatcher for tenant {TenantId}", request.RequestId, request.TenantId);
				await _tenantDispatcher.DispatchRequestAsync(request);
			}
		}
	}
}
