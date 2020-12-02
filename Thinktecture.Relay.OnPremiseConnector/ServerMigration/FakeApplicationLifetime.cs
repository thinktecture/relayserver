using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Thinktecture.Relay.OnPremiseConnector.ServerMigration
{
	internal class FakeApplicationLifetime : IApplicationLifetime
	{
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public CancellationToken ApplicationStarted => CancellationToken.None;
		public CancellationToken ApplicationStopping => CancellationToken.None;
		public CancellationToken ApplicationStopped => _cts.Token;

		public void StopApplication() => _cts.Cancel();
	}
}
