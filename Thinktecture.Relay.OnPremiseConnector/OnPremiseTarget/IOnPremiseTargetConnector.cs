using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetConnector
	{
		Task<IOnPremiseTargetResponse> GetResponseFromLocalTargetAsync(string url, IOnPremiseTargetRequest request, string relayedRequestHeader);
	}
}
