using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
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
		/// Initializes a new instance of <see cref="RelayTargetService{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public RelayTargetService(IOptions<RelayTargetServiceOptions<TRequest, TResponse>> options)
			=> _targets = options?.Value.Registrations ?? throw new ArgumentNullException(nameof(options));

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
