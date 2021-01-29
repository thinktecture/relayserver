using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryServerTransport<TResponse, TAcknowledge> : IServerTransport<TResponse, TAcknowledge>
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly IResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;

		public InMemoryServerTransport(IResponseCoordinator<TResponse> responseCoordinator,
			IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator)
		{
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
		}

		public int? BinarySizeThreshold { get; } = null; // no limit

		public Task DispatchResponseAsync(TResponse response) => _responseCoordinator.ProcessResponseAsync(response);

		public Task DispatchAcknowledgeAsync(TAcknowledge request) => _acknowledgeCoordinator.ProcessAcknowledgeAsync(request);
	}
}
