using System.Collections.ObjectModel;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <summary>
	/// A registry for <see cref="IRelayTarget{TRequest,TResponse}"/>s.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class RelayTargetRegistry<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// The registered <see cref="IRelayTarget{TRequest,TResponse}"/>s keyed by their id.
		/// </summary>
		public ReadOnlyDictionary<string, RelayTargetRegistration<TRequest, TResponse>> Targets { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="options">The <see cref="RelayConnectorOptions{TRequest,TResponse}"/>.</param>
		public RelayTargetRegistry(RelayConnectorOptions<TRequest, TResponse> options)
			=> Targets = new ReadOnlyDictionary<string, RelayTargetRegistration<TRequest, TResponse>>(options.Targets);

		// TODO add possibility to add/remove targets
	}
}
