using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Diagnostics;
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
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/>.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> AddRelayServer(this IServiceCollection services,
			Action<RelayServerOptions>? configure = null)
			=> services.AddRelayServer<ClientRequest, TargetResponse, AcknowledgeRequest>(configure);

		/// <summary>
		/// Adds the server to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="configure">An optional configuration action.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/>.</returns>
		public static IRelayServerBuilder<TRequest, TResponse, TAcknowledge> AddRelayServer<TRequest, TResponse, TAcknowledge>(
			this IServiceCollection services, Action<RelayServerOptions>? configure = null)
			where TRequest : IClientRequest, new()
			where TResponse : class, ITargetResponse, new()
			where TAcknowledge : IAcknowledgeRequest
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

			services.TryAddScoped<RelayMiddleware<TRequest, TResponse, TAcknowledge>>();
			services.TryAddScoped<IRelayContext<TRequest, TResponse>, RelayContext<TRequest, TResponse>>();
			services.TryAddScoped<IRelayRequestLogger<TRequest, TResponse>, RelayRequestLogger<TRequest, TResponse>>();
			services.TryAddScoped<DiscoveryDocumentBuilder>();

			services.TryAddSingleton<IRelayTargetResponseWriter<TResponse>, RelayTargetResponseWriter<TResponse>>();
			services.TryAddSingleton<IRelayClientRequestFactory<TRequest>, RelayClientRequestFactory<TRequest>>();
			services.TryAddSingleton<IRequestCoordinator<TRequest>, RequestCoordinator<TRequest>>();
			services.TryAddSingleton<IResponseCoordinator<TResponse>, ResponseCoordinator<TResponse>>();
			services.TryAddSingleton<IResponseDispatcher<TResponse>, ResponseDispatcher<TResponse, TAcknowledge>>();
			services.TryAddSingleton<IAcknowledgeCoordinator<TAcknowledge>, AcknowledgeCoordinator<TRequest, TAcknowledge>>();
			services.TryAddSingleton<IAcknowledgeDispatcher<TAcknowledge>, AcknowledgeDispatcher<TResponse, TAcknowledge>>();
			services.TryAddSingleton<IOriginStatisticsWriter, OriginStatisticsWriter>();
			services.TryAddSingleton<IConnectionStatisticsWriter, ConnectionStatisticsWriter>();

			services.AddSingleton<RelayServerContext>();
			services.AddSingleton<ConnectorRegistry<TRequest>>();

			services.AddHostedService<ServerStatisticsWriter>();

			services.AddHealthChecks()
				.AddCheck<TransportHealthCheck>("Transport", tags: new[] { "ready" });

			return new RelayServerBuilder<TRequest, TResponse, TAcknowledge>(services);
		}
	}
}
