using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.SignalR
{
	/// <summary>
	/// Interface for a handler when something is received from an on-premise connector via the SignalR connection.
	/// </summary>
	public interface IOnPremisesConnectionOnReceivedHandler
	{
		/// <summary>
		/// This method will be called when an on-premise sends back data via the SignalR connection.
		/// </summary>
		/// <param name="connectionId">The id of the connection.</param>
		/// <param name="data">The data sent.</param>
		/// <returns><see cref="Task"/>.</returns>
		Task HandleAsync(string connectionId, string data);
	}
}
