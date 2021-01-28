using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Transport
{
	public interface IResponseTransport<in T>
		where T : ITargetResponse
	{
		Task TransportAsync(T response, CancellationToken cancellationToken = default);
	}
}
