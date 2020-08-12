using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Transport;
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
		/// Adds the in-memory server routing. Use this only for single server or testing scenarios.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddInMemoryServerRouting<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.AddSingleton<IServerDispatcher<TResponse>, InMemoryServerDispatcher<TResponse>>();
			builder.Services.AddSingleton<IServerHandler<TResponse>, InMemoryServerHandler<TResponse>>();
			builder.Services.AddSingleton<ITenantDispatcher<TRequest>, InMemoryTenantDispatcher<TRequest>>();
			builder.Services.AddSingleton<ITenantHandlerFactory<TRequest, TResponse>, InMemoryTenantHandlerFactory<TRequest, TResponse>>();

			return builder;
		}

		/// <summary>
		/// Adds the in-memory body store. Use this only for single server or testing scenarios.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		/// <remarks>This could harm the memory usage of the server.</remarks>
		public static IRelayServerBuilder<TRequest, TResponse> AddInMemoryBodyStore<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.AddSingleton<IBodyStore, InMemoryBodyStore>();

			return builder;
		}

		/// <summary>
		/// Adds the file-based body store.
		/// </summary>
		/// <typeparam name="TRequest"></typeparam>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <param name="configure">A configure callback for setting the <see cref="FileBodyStoreOptions"/>.</param>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		/// <remarks>Use a shared location between all server instances.</remarks>
		public static IRelayServerBuilder<TRequest, TResponse> AddFileBodyStore<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder, Action<FileBodyStoreOptions> configure)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.Configure(configure);
			builder.Services.AddTransient<IValidateOptions<FileBodyStoreOptions>, FileBodyStoreOptionsValidator>();
			builder.Services.AddSingleton<IBodyStore, FileBodyStore>();

			return builder;
		}
	}
}
