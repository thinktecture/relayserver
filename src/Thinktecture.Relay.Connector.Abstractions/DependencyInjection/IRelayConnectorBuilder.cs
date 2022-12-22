using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.DependencyInjection;

/// <summary>
/// Connector builder interface.
/// </summary>
// ReSharper disable UnusedTypeParameter; (this makes method call signatures cleaner)
public interface IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge>
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
