using System.Collections.Generic;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <summary>
	/// Options for the relay target service.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class RelayTargetServiceOptions<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Gets the relay target registrations.
		/// </summary>
		public List<RelayTargetRegistration<TRequest, TResponse>> Registrations { get; } =
			new List<RelayTargetRegistration<TRequest, TResponse>>();
	}
}
