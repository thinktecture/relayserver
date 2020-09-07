using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Factories;
using Thinktecture.Relay.Server.HealthChecks;
using Thinktecture.Relay.Server.Middleware;
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
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse}"/>.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse> AddRelayServer(this IServiceCollection services)
			=> services.AddRelayServer<ClientRequest, TargetResponse>();

		/// <summary>
		/// Adds the server to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddRelayServer<TRequest, TResponse>(this IServiceCollection services)
			where TRequest : IClientRequest, new()
			where TResponse : ITargetResponse, new()
		{
			services.AddAuthorization(configure =>
			{
				configure.AddPolicy(Constants.DefaultAuthenticationPolicy, policyBuilder =>
				{
					policyBuilder
						.RequireAuthenticatedUser()
						.RequireClaim("client_id")
						.RequireClaim("scope", Constants.DefaultAuthenticationScope);
				});
			});

			services.TryAddScoped<IRelayClientRequestFactory<TRequest>, RelayClientRequestFactory<TRequest>>();
			services.TryAddScoped<RelayMiddleware<TRequest, TResponse>>();
			services.TryAddScoped<DiscoveryDocumentBuilder>();
			services.TryAddScoped<IRelayContext<TRequest, TResponse>, RelayContext<TRequest, TResponse>>();
			services.TryAddSingleton<RelayServerContext>();
			services.TryAddSingleton<ResponseCoordinator<TRequest, TResponse>>();
			services.TryAddSingleton<TenantConnectorAdapterRegistry<TRequest, TResponse>>();
			services.TryAddSingleton<IRelayTargetResponseWriter<TResponse>, RelayTargetResponseWriter<TResponse>>();
			services.TryAddSingleton<AcknowledgeCoordinator<TRequest, TResponse>>();

			services.AddHealthChecks()
				.AddCheck<TransportHealthCheck>("Transport", tags: new[] { "ready" });

			return new RelayServerBuilder<TRequest, TResponse>(services);
		}
	}
}
