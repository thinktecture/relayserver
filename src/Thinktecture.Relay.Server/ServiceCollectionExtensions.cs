using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Factories;
using Thinktecture.Relay.Server.HealthChecks;
using Thinktecture.Relay.Server.Middleware;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Services;
using Thinktecture.Relay.Server.Transport;
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
		/// Adds the server to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configure">An optional configuration action.</param>
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse}"/>.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse> AddRelayServer(this IServiceCollection services,
			Action<RelayServerOptions> configure = null)
			=> services.AddRelayServer<ClientRequest, TargetResponse>(configure);

		/// <summary>
		/// Adds the server to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configure">An optional configuration action.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddRelayServer<TRequest, TResponse>(this IServiceCollection services,
			Action<RelayServerOptions> configure = null)
			where TRequest : IClientRequest, new()
			where TResponse : class, ITargetResponse, new()
		{
			if (configure != null)
			{
				services.Configure(configure);
			}

			services.AddHttpContextAccessor();

			services.AddAuthorization(configureAuthorization =>
			{
				configureAuthorization.AddPolicy(Constants.DefaultAuthenticationPolicy, configurePolicy =>
				{
					configurePolicy
						.RequireAuthenticatedUser()
						.RequireClaim("client_id")
						.RequireClaim("scope", Constants.DefaultAuthenticationScope);
				});
			});

			services.TryAddScoped<DiscoveryDocumentBuilder>();
			services.TryAddScoped<RelayMiddleware<TRequest, TResponse>>();
			services.TryAddScoped<IRelayClientRequestFactory<TRequest>, RelayClientRequestFactory<TRequest>>();
			services.TryAddScoped<IRelayContext<TRequest, TResponse>, RelayContext<TRequest, TResponse>>();
			services.TryAddScoped<IRequestCoordinator<TRequest>, RequestCoordinator<TRequest, TResponse>>();
			services.TryAddScoped<IRelayTargetResponseWriter<TResponse>, RelayTargetResponseWriter<TResponse>>();
			services.TryAddScoped<IRelayRequestLogger<TRequest, TResponse>, RelayRequestLogger<TRequest, TResponse>>();

			services.AddSingleton<RelayServerContext>();
			services.AddSingleton<TenantConnectorAdapterRegistry<TRequest, TResponse>>();

			services.TryAddSingleton<IResponseCoordinator<TResponse>, ResponseCoordinator<TRequest, TResponse>>();
			services.TryAddSingleton<IAcknowledgeCoordinator, AcknowledgeCoordinator<TRequest, TResponse>>();
			services.TryAddSingleton<IOriginStatisticsWriter, OriginStatisticsWriter>();
			services.TryAddSingleton<IConnectionStatisticsWriter, ConnectionStatisticsWriter>();

			services.AddHostedService<ServerStatisticsWriter>();

			services.AddHealthChecks()
				.AddCheck<TransportHealthCheck>("Transport", tags: new[] { "ready", });

			return new RelayServerBuilder<TRequest, TResponse>(services);
		}
	}
}
