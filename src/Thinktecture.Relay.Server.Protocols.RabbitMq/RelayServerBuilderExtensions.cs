using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Thinktecture.Relay;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Protocols.RabbitMq;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on ServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayServerBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayServerBuilderExtensions
	{
		/// <summary>
		/// Adds the <see cref="ITenantDispatcher{TRequest}"/> and ? based on Rabbit MQ.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddRabbitMqTenantRouting<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.TryAddSingleton<ITenantDispatcher<TRequest>, TenantDispatcher<TRequest>>();
			builder.Services.TryAddSingleton<ITenantHandlerFactory<TRequest, TResponse>, TenantHandlerFactory<TRequest, TResponse>>();
			builder.Services.AddRabbitMq();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="IServerDispatcher{TResponse}"/> and <see cref="IServerHandler{TResponse}"/> based on Rabbit MQ.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddRabbitMqServerRouting<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.TryAddSingleton<IServerDispatcher<TResponse>, ServerDispatcher<TResponse>>();
			builder.Services.TryAddSingleton<IServerHandler<TResponse>, ServerHandler<TResponse>>();
			builder.Services.AddRabbitMq();

			return builder;
		}

		private static void AddRabbitMq(this IServiceCollection services)
		{
			services.TryAddSingleton(provider =>
			{
				var relayServerContext = provider.GetRequiredService<RelayServerContext>();
				var factory = new ConnectionFactory(); // TODO configure anything here

				return factory.CreateConnection($"Relay Server {relayServerContext.OriginId}");
			});
			services.AddSingleton<ModelFactory>();
		}
	}
}
