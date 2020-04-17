using System.Threading;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// A context containing processing information and data for one relay task.
	/// </summary>
	public interface IRelayContext
	{
		/// <summary>
		/// The client request.
		/// </summary>
		IRelayClientRequest ClientRequest { get; set; }

		/// <summary>
		/// The target response.
		/// <remarks>Setting this in an interceptor prevents requesting any target by default.</remarks>
		/// <seealso cref="ForceConnectorDelivery"/>
		/// </summary>
		IRelayTargetResponse TargetResponse { get; set; }

		/// <summary>
		/// Indicates if at least one connector is available for processing the <see cref="ClientRequest"/>
		/// </summary>
		bool IsConnectorAvailable { get; }

		/// <summary>
		/// Indicates that the regardless of an already available <see cref="TargetResponse"/> the <see cref="ClientRequest"/> should be send
		/// to a connector for further processing by a target (ignoring the results).
		/// </summary>
		bool ForceConnectorDelivery { get; set; }

		/// <summary>
		/// A token signaling the cancellation of the task.
		/// </summary>
		CancellationToken CancellationToken { get; }
	}
}
