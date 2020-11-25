using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <summary>
	/// A service for retrieving a <see cref="RelayTargetRegistration{TRequest,TResponse}"/>.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class RelayTargetService<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly List<RelayTargetRegistration<TRequest, TResponse>> _targets;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayTargetService{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="serviceOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="connectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public RelayTargetService(IOptions<RelayTargetServiceOptions<TRequest, TResponse>> serviceOptions,
			IOptions<RelayConnectorOptions> connectorOptions)
		{
			_targets = serviceOptions?.Value.Registrations ?? throw new ArgumentNullException(nameof(serviceOptions));

			if (connectorOptions == null)
			{
				throw new ArgumentNullException(nameof(connectorOptions));
			}

			if (connectorOptions.Value.WebTargets?.Count > 0)
			{
				_targets.AddRange(connectorOptions.Value.WebTargets.Select(kvp
					=> new RelayTargetRegistration<TRequest, TResponse, RelayWebTarget<TRequest, TResponse>>(kvp.Key, kvp.Value.Timeout,
						new Uri(kvp.Value.Url))));
			}
		}

		/// <summary>
		/// Gets the target registration by id or as fallback the <see cref="Constants.RelayTargetCatchAllId"/> if available.
		/// </summary>
		/// <param name="id">The unique id of the target.</param>
		/// <returns>A <see cref="RelayTargetRegistration{TRequest,TResponse}"/> or null if not found.</returns>
		public RelayTargetRegistration<TRequest, TResponse> GetRelayTargetRegistration(string id)
			=> TryGetTargetInternal(id, out var registration) || TryGetTargetInternal(Constants.RelayTargetCatchAllId, out registration)
				? registration
				: null;

		private bool TryGetTargetInternal(string id, out RelayTargetRegistration<TRequest, TResponse> registration)
		{
			registration = _targets.FirstOrDefault(target => target.Id == id);
			return registration != null;
		}
	}
}
