using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	internal class RelayServerBuilder<TRequest, TResponse> : IRelayServerBuilder<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		public IServiceCollection Services { get; }

		public RelayServerBuilder(IServiceCollection services) => Services = services ?? throw new ArgumentNullException(nameof(services));
	}
}
