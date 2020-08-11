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
	}
}
