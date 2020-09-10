using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class OnPremisesConnectionOnReceivedHandler : IOnPremisesConnectionOnReceivedHandler
	{
		public Task HandleAsync(string connectionId, string data)
		{
			return Task.CompletedTask;
		}
	}
}
