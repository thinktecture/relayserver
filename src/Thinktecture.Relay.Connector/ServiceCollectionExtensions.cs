using System;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Thinktecture.Relay;
using Thinktecture.Relay.Connector;
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
		/// <param name="configure">A configure callback for setting the <see cref="RelayConnectorOptions{ClientRequest,TargetResponse}"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddRelayConnector(this IServiceCollection services,
			Action<RelayConnectorOptions<ClientRequest, TargetResponse>> configure)
			=> services.AddRelayConnector<ClientRequest, TargetResponse>(configure);

		/// <summary>
		/// Adds the connector to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configure">A configure callback for setting the <see cref="RelayConnectorOptions{TRequest,TResponse}"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddRelayConnector<TRequest, TResponse>(this IServiceCollection services,
			Action<RelayConnectorOptions<TRequest, TResponse>> configure)
			where TRequest : IClientRequest, new()
			where TResponse : ITargetResponse, new()
		{
			var builder = new RelayConnectorBuilder(services);

			builder.Services.Configure(configure);

			builder.Services.AddTransient<IPostConfigureOptions<RelayConnectorOptions<TRequest, TResponse>>,
				RelayConnectorPostConfigureOptions<TRequest, TResponse>>();
			builder.Services.AddTransient<IConfigurationRetriever<DiscoveryDocument>, RelayServerConfigurationRetriever>();
			builder.Services.AddTransient<IConfigureOptions<AccessTokenManagementOptions>,
				ConfigureAccessTokenManagementOptions<TRequest, TResponse>>();

			builder.Services.AddAccessTokenManagement();
			builder.Services
				.AddHttpClient(Constants.RelayServerHttpClientName, (provider, client) =>
				{
					var options = provider.GetRequiredService<IOptions<RelayConnectorOptions<TRequest, TResponse>>>();
					client.BaseAddress = options.Value.RelayServerBaseUri;
					// TODO set timeouts
				})
				.AddClientAccessTokenHandler();

			builder.Services.TryAddSingleton<IRelayTargetResponseFactory<TResponse>, RelayTargetResponseFactory<TResponse>>();
			builder.Services.AddSingleton<RelayTargetRegistry<TRequest, TResponse>>();
			builder.Services.AddSingleton<RelayClientRequestHandler<TRequest, TResponse>>();

			return builder;
		}
	}
}
