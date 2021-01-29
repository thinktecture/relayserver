using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryTenantHandler : ITenantHandler
	{
		public static readonly ITenantHandler Noop = new InMemoryTenantHandler();

		public Task AcknowledgeAsync(string acknowledgeId, CancellationToken cancellationToken = default) => Task.CompletedTask;
	}
}
