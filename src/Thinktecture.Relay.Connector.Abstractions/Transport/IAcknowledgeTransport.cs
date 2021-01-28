using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Connector.Transport
{
	public interface IAcknowledgeTransport<in T>
		where T : IAcknowledgeRequest
	{
		Task TransportAsync(T request, CancellationToken cancellationToken = default);
	}
}
