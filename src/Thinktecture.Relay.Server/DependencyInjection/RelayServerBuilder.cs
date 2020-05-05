using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	internal class RelayServerBuilder<TRequest, TResponse> : IRelayServerBuilder<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		public IServiceCollection Services { get; }

		public RelayServerBuilder(IServiceCollection services) => Services = services;
	}
}
