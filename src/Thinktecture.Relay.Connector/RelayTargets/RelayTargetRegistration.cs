using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <summary>
	/// A registration of an <see cref="IRelayTarget{TRequest,TResponse}"/>.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class RelayTargetRegistration<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		internal Func<IServiceProvider, IRelayTarget<TRequest, TResponse>> Factory { get; }

		internal RelayTargetRegistration(IRelayTargetOptions options, Type type)
			=> Factory = provider => (IRelayTarget<TRequest, TResponse>)ActivatorUtilities.CreateInstance(provider, type, options);
	}
}
