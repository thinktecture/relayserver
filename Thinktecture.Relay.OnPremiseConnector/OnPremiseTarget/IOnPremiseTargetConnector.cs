using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    internal interface IOnPremiseTargetConnector
    {
        Task<IOnPremiseTargetResponse> GetResponseAsync(string url, IOnPremiseTargetRequest request);
    }
}