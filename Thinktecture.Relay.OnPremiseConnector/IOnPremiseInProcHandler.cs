using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector
{
    public interface IOnPremiseInProcHandler
    {
        Task ProcessRequest(IOnPremiseTargetRequest request, IOnPremiseTargetResponse response, CancellationToken cancellationToken);
    }
}
