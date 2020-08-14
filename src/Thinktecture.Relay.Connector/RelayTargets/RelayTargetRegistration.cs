using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	internal class RelayTargetRegistration<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		public string Id { get; }

		public Func<IServiceProvider, IRelayTarget<TRequest, TResponse>> Factory { get; }

		public RelayTargetRegistration(Type target, string id, IRelayTargetOptions options)
		{
			Id = id;
			Factory = provider => (IRelayTarget<TRequest, TResponse>)ActivatorUtilities.CreateInstance(provider, target, options);
		}
	}
}
