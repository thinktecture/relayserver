// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)

using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Abstractions;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.Factories;
using Thinktecture.Relay.Server.Middleware;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRelayServer(this IServiceCollection serviceCollection)
		{
			return serviceCollection.AddRelayServer<ClientRequest>();
		}

		public static IServiceCollection AddRelayServer<TRequest>(this IServiceCollection serviceCollection)
			where TRequest : ITransportClientRequest, new()
		{
			serviceCollection.TryAddScoped<ITransportClientRequestFactory<TRequest>, TransportClientRequestFactory<TRequest>>();
			serviceCollection.TryAddScoped<RelayMiddleware<TRequest>>();

			return serviceCollection;
		}
	}
}
