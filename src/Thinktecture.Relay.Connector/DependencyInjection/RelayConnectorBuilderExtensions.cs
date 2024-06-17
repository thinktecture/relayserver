using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.
/// </summary>
public static class RelayConnectorBuilderExtensions
{
	/// <summary>
	/// Adds the <see cref="PingTarget{TRequest,TResponse}"/>.
	/// </summary>
	/// <remarks>The default target key is "$ping".</remarks>
	/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</param>
	/// <param name="targetKey">The target key for the <see cref="PingTarget{TRequest,TResponse}"/>.</param>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
	/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</returns>
	public static IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> AddPingTarget<TRequest, TResponse,
		TAcknowledge>(this IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> builder, string targetKey = "$ping")
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
		where TAcknowledge : IAcknowledgeRequest
	{
		builder.AddTarget<TRequest, TResponse, TAcknowledge, PingTarget<TRequest, TResponse>>(targetKey);

		return builder;
	}

	/// <summary>
	/// Adds the <see cref="EchoTarget{TRequest,TResponse}"/>.
	/// </summary>
	/// <remarks>The default target key is "$echo" and should only be added for debugging purposes.</remarks>
	/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</param>
	/// <param name="targetKey">The target key for the <see cref="EchoTarget{TRequest,TResponse}"/>.</param>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
	/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</returns>
	public static IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> AddEchoTarget<TRequest, TResponse,
		TAcknowledge>(this IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> builder, string targetKey = "$echo")
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
		where TAcknowledge : IAcknowledgeRequest
	{
		builder.AddTarget<TRequest, TResponse, TAcknowledge, EchoTarget<TRequest, TResponse>>(targetKey);

		return builder;
	}
}
