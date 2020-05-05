using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	internal class RelayTargetRegistration<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		public IRelayTargetOptions Options { get; }
		public Func<IServiceProvider, IRelayTarget<TRequest, TResponse>> Factory { get; }

		public RelayTargetRegistration(IRelayTargetOptions options, Type type)
		{
			Options = options;
			Factory = provider => (IRelayTarget<TRequest, TResponse>)ActivatorUtilities.CreateInstance(provider, type, options);
		}
	}
}
