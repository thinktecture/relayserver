using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	internal class RelayServerBuilder<TRequest, TResponse, TAcknowledge> : IRelayServerBuilder<TRequest, TResponse, TAcknowledge>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		public IServiceCollection Services { get; }

		public RelayServerBuilder(IServiceCollection services) => Services = services ?? throw new ArgumentNullException(nameof(services));
	}
}
