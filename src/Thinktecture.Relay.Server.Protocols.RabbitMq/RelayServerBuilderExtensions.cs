using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
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
		/// Adds the routing based on Rabbit MQ.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <param name="configure">A configure callback for setting the <see cref="RabbitMqOptions"/>.</param>
		/// <param name="useServerRouting">Enables Rabbit MQ for server-to-server communication.</param>
		/// <param name="useTenantRouting">Enables Rabbit MQ for server-to-tenant communication.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddRabbitMqRouting<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder, Action<RabbitMqOptions> configure, bool useServerRouting = true,
			bool useTenantRouting = true)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.TryAddSingleton<IServerDispatcher<TResponse>, ServerDispatcher<TResponse>>();
			builder.Services.TryAddSingleton<IServerHandler<TResponse>, ServerHandler<TResponse>>();
			builder.Services.AddRabbitMq<TRequest, TResponse>(useServerRouting, useTenantRouting, configure);

			return builder;
		}

		private static void AddRabbitMq<TRequest, TResponse>(this IServiceCollection services, bool useServerRouting, bool useTenantRouting,
			Action<RabbitMqOptions> configure)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			services.Configure(configure);

			if (useServerRouting)
			{
				services.TryAddSingleton<IServerDispatcher<TResponse>, ServerDispatcher<TResponse>>();
				services.TryAddSingleton<IServerHandler<TResponse>, ServerHandler<TResponse>>();
			}

			if (useTenantRouting)
			{
				services.TryAddSingleton<ITenantDispatcher<TRequest>, TenantDispatcher<TRequest>>();
				services.TryAddSingleton<ITenantHandlerFactory<TRequest, TResponse>, TenantHandlerFactory<TRequest, TResponse>>();
			}

			services.TryAddSingleton<IConnection>(provider =>
			{
				var relayServerContext = provider.GetRequiredService<RelayServerContext>();
				var options = provider.GetRequiredService<IOptions<RabbitMqOptions>>();

				var factory = new ConnectionFactory
				{
					EndpointResolverFactory = endpoints => new RoundRobinEndpointResolver(endpoints),
					Uri = new Uri(options.Value.Uri),
				};

				var connection = (IAutorecoveringConnection)factory.CreateConnection(
					AmqpTcpEndpoint.ParseMultiple(options.Value.ClusterHosts ?? factory.Uri.Host),
					$"RelayServer {relayServerContext.OriginId}");

				// TODO better logging
				connection.RecoverySucceeded += (sender, args) => Console.WriteLine("Connection Recovered!");
				connection.ConnectionShutdown += (sender, args) => Console.WriteLine("Connection Shutdown! {0}", args.ReplyText);

				return connection;
			});
			services.AddSingleton<ModelFactory>();
		}
	}
}
