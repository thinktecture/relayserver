using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface ITokenExpiryChecker
	{
		Task CheckTokenExpiry(IRelayServerConnection connection);
	}
}
