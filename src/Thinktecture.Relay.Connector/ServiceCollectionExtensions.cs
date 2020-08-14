using System;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Thinktecture.Relay;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.Authentication;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds the connector to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configure">A configure callback for setting the <see cref="RelayConnectorOptions"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<ClientRequest, TargetResponse> AddRelayConnector(this IServiceCollection services,
			Action<RelayConnectorOptions> configure)
			=> services.AddRelayConnector<ClientRequest, TargetResponse>(configure);

		/// <summary>
		/// Adds the connector to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configure">A configure callback for setting the <see cref="RelayConnectorOptions"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddRelayConnector<TRequest, TResponse>(this IServiceCollection services,
			Action<RelayConnectorOptions> configure)
			where TRequest : IClientRequest, new()
			where TResponse : ITargetResponse, new()
		{
			var builder = new RelayConnectorBuilder<TRequest, TResponse>(services);

			builder.Services.Configure(configure);

			builder.Services.AddTransient<IPostConfigureOptions<RelayConnectorOptions>,
				RelayConnectorPostConfigureOptions>();
			builder.Services.AddTransient<IConfigurationRetriever<DiscoveryDocument>, RelayServerConfigurationRetriever>();
			builder.Services.AddTransient<IConfigureOptions<AccessTokenManagementOptions>,
				ConfigureAccessTokenManagementOptions>();

			builder.Services.AddAccessTokenManagement();
			builder.Services.TryAddTransient<IAccessTokenProvider, AccessTokenProvider>();
			builder.Services
				.AddHttpClient(Constants.RelayServerHttpClientName, (provider, client) =>
				{
					var options = provider.GetRequiredService<IOptions<RelayConnectorOptions>>();
					client.BaseAddress = options.Value.RelayServerBaseUri;
					// TODO set timeouts
				})
				.AddClientAccessTokenHandler();

			builder.Services.TryAddSingleton<IRelayTargetResponseFactory<TResponse>, RelayTargetResponseFactory<TResponse>>();
			builder.Services.AddSingleton<IClientRequestHandler<TRequest, TResponse>, ClientRequestHandler<TRequest, TResponse>>();
			builder.Services.AddSingleton<RelayConnector>();

			return builder;
		}
	}
}
