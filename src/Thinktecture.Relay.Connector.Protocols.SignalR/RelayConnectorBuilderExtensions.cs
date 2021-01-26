using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Protocols.SignalR;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds the connector transport based on SignalR.
		/// </summary>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <param name="builder">An instance of the <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddSignalRConnectorTransport<TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
		{
			builder.Services.AddTransient<HubConnectionFactory>();
			builder.Services.AddTransient<IConnectorConnection, ConnectorConnection<TRequest, TResponse>>();
			builder.Services.AddSingleton<DiscoveryDocumentRetryPolicy>();

			return builder;
		}
	}
}
