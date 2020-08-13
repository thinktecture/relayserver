using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <summary>
	/// A client request handler determining the <see cref="IRelayTarget{TRequest,TResponse}"/> handling the request.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class RelayClientRequestHandler<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayClientRequestHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="relayTargetRegistry">The <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public RelayClientRequestHandler(RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry, IServiceProvider serviceProvider)
		{
			_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <summary>
		/// Called when a request was received.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response.</returns>
		public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (!TryCreateTarget(request.Target, out var target))
			{
				request.Url = $"/{request.Target}{request.Url}";
				request.Target = null;

				if (!TryCreateTarget(RelayConnectorBuilder.RelayTargetCatchAllId, out target))
				{
					return default;
				}
			}

			try
			{
				var response = await target.HandleAsync(request, cancellationToken);
				// TODO optional post body first
				return response;
			}
			finally
			{
				(target as IDisposable)?.Dispose();
			}
		}

		private bool TryCreateTarget(string id, out IRelayTarget<TRequest, TResponse> target)
		{
			if (_relayTargetRegistry.Targets.TryGetValue(id, out var registration))
			{
				target = registration.Factory(_serviceProvider);
				return true;
			}

			target = null;
			return false;
		}
	}
}
