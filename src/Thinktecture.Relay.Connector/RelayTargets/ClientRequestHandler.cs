using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc />
	internal class ClientRequestHandler<TRequest, TResponse> : IClientRequestHandler<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly Dictionary<string, RelayTargetRegistration<TRequest, TResponse>> _targets
			= new Dictionary<string, RelayTargetRegistration<TRequest, TResponse>>(StringComparer.InvariantCultureIgnoreCase);

		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of <see cref="ClientRequestHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		/// <param name="targets">The registered <see cref="IRelayTarget{TRequest,TResponse}"/>s.</param>
		public ClientRequestHandler(IServiceProvider serviceProvider, IEnumerable<RelayTargetRegistration<TRequest, TResponse>> targets)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			foreach (var target in targets)
			{
				_targets[target.Id] = target;
			}
		}

		/// <inheritdoc />
		public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (!TryGetTarget(request.Target, out var target) && !TryGetTarget(RelayConnectorBuilder.RelayTargetCatchAllId, out target))
			{
				return default;
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

		private bool TryGetTarget(string id, out IRelayTarget<TRequest, TResponse> target)
		{
			if (_targets.TryGetValue(id, out var registration))
			{
				target = registration.Factory(_serviceProvider);
				return true;
			}

			target = null;
			return false;
		}
	}
}
