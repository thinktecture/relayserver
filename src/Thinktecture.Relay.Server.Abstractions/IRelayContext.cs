using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// A context containing processing information and data for one relay task.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IRelayContext<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// The client request.
		/// </summary>
		TRequest ClientRequest { get; set; }

		/// <summary>
		/// The target response.
		/// </summary>
		/// <remarks>Setting this to an instance in an interceptor prevents requesting any target by default.</remarks>
		/// <seealso cref="ForceConnectorDelivery"/>
		TResponse TargetResponse { get; set; }

		/// <summary>
		/// Indicates if at least one connector is available for processing the <see cref="ClientRequest"/>
		/// </summary>
		bool IsConnectorAvailable { get; }

		/// <summary>
		/// Indicates that the regardless of an already available <see cref="TargetResponse"/> the <see cref="ClientRequest"/> should be send
		/// to a connector for further processing by a target (ignoring the results).
		/// </summary>
		bool ForceConnectorDelivery { get; set; }
	}
}
