using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Docker.Interceptors;

public class ConnectorFinishedAcknowledgementModeInterceptor : IClientRequestInterceptor<ClientRequest, TargetResponse>
{
	public Task OnRequestReceivedAsync(IRelayContext<ClientRequest, TargetResponse> context,
		CancellationToken cancellationToken = default)
	{
		context.ClientRequest.AcknowledgeMode = AcknowledgeMode.Manual;
		return Task.CompletedTask;
	}
}
