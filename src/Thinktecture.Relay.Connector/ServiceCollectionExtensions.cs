using System;
using System.Net.Http;
using System.Threading;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.Authentication;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Connector.Targets;
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

			builder.Services
				.AddTransient<IConfigureOptions<RelayConnectorOptions>, RelayConnectorConfigureOptions>()
				.AddTransient<IPostConfigureOptions<RelayConnectorOptions>, RelayConnectorPostConfigureOptions<TRequest, TResponse>>()
				.AddTransient<IValidateOptions<RelayConnectorOptions>, RelayConnectorValidateOptions>();
			builder.Services.AddTransient<IConfigureOptions<AccessTokenManagementOptions>, AccessTokenManagementConfigureOptions>();

			builder.Services.AddAccessTokenManagement();
			builder.Services.TryAddTransient<IAccessTokenProvider, AccessTokenProvider>();
			builder.Services
				.AddHttpClient(Constants.HttpClientNames.RelayServer, (provider, client) =>
				{
					var options = provider.GetRequiredService<IOptions<RelayConnectorOptions>>();
					client.BaseAddress = options.Value.RelayServerBaseUri;
					client.Timeout = options.Value.DiscoveryDocument.EndpointTimeout;
				})
				.AddClientAccessTokenHandler();

			builder.Services
				.AddHttpClient(Constants.HttpClientNames.RelayWebTargetDefault, client => client.Timeout = Timeout.InfiniteTimeSpan)
				.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false });
			builder.Services
				.AddHttpClient(Constants.HttpClientNames.RelayWebTargetFollowRedirect, client => client.Timeout = Timeout.InfiniteTimeSpan)
				.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseCookies = false });

			builder.Services.TryAddTransient<IClientRequestHandler<TRequest, TResponse>, ClientRequestHandler<TRequest, TResponse>>();
			builder.Services.TryAddTransient<IClientRequestWorker<TRequest, TResponse>, ClientRequestWorker<TRequest, TResponse>>();

			builder.Services.AddSingleton<RelayTargetRegistry<TRequest, TResponse>>();
			builder.Services.AddSingleton<RelayConnector>();

			return builder;
		}
	}
}
