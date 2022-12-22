using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.DependencyInjection;

/// <summary>
/// Server builder interface.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
// ReSharper disable UnusedTypeParameter; (this makes method call signatures cleaner)
public interface IRelayServerBuilder<TRequest, TResponse, TAcknowledge>
// ReSharper restore UnusedTypeParameter
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
	where TAcknowledge : IAcknowledgeRequest
{
	/// <summary>
	/// Gets the application service collection.
	/// </summary>
	IServiceCollection Services { get; }
}
