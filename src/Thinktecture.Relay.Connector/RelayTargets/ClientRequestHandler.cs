using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc />
	public class ClientRequestHandler<TRequest, TResponse> : IClientRequestHandler<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of <see cref="ClientRequestHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="relayTargetRegistry">The <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public ClientRequestHandler(RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry, IServiceProvider serviceProvider)
		{
			_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <inheritdoc />
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
