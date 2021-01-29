using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Protocols.SignalR;
using Thinktecture.Relay.Connector.Transport;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		private class ConnectorTransportLimit : IConnectorTransportLimit
		{
			public int? BinarySizeThreshold { get; } = 16 * 1024; // 16kb
		}

		/// <summary>
		/// Adds the connector transport based on SignalR.
		/// </summary>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
		/// <param name="builder">An instance of the <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> AddSignalRConnectorTransport<TRequest, TResponse,
			TAcknowledge>(this IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> builder)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
			where TAcknowledge : IAcknowledgeRequest
		{
			builder.Services.AddSingleton<HubConnectionFactory>();
			builder.Services.AddSingleton(provider => provider.GetRequiredService<HubConnectionFactory>().Create());

			builder.Services.AddSingleton<IConnectorTransportLimit, ConnectorTransportLimit>();
			builder.Services.AddSingleton<IConnectorConnection, ConnectorConnection<TRequest, TResponse, TAcknowledge>>();
			builder.Services.AddSingleton<DiscoveryDocumentRetryPolicy>();

			builder.Services.AddSingleton<IResponseTransport<TResponse>, ResponseTransport<TResponse>>();
			builder.Services.AddSingleton<IAcknowledgeTransport<TAcknowledge>, AcknowledgeTransport<TAcknowledge>>();

			return builder;
		}
	}
}
