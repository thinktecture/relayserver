using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Docker
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class ClientRequestInterceptor : IClientRequestInterceptor<ClientRequest, TargetResponse>
	{
		public Task OnRequestReceivedAsync(IRelayContext<ClientRequest, TargetResponse> context,
			CancellationToken cancellationToken = default)
		{
			context.ClientRequest.HttpHeaders.Add("X-MACHINE-NAME", new[] { Environment.MachineName });
			return Task.CompletedTask;
		}
	}
}
