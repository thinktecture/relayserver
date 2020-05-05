using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.DependencyInjection;
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
		/// <param name="discoveryDocument">The <see cref="Uri"/> to the discovery document of the server.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddRelayConnector(this IServiceCollection services, Uri discoveryDocument)
		{
			return services.AddRelayConnector<ClientRequest, TargetResponse>(discoveryDocument);
		}

		/// <summary>
		/// Adds the connector to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="discoveryDocument">The <see cref="Uri"/> to the discovery document of the server.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddRelayConnector<TRequest, TResponse>(this IServiceCollection services, Uri discoveryDocument)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse, new()
		{
			var builder = new RelayConnectorBuilder();

			builder.Services.Configure<RelayConnectorOptions<TRequest, TResponse>>(options =>
			{
				options.DiscoveryDocument = discoveryDocument;
			});

			builder.Services.TryAddScoped<IRelayTargetResponseFactory<TResponse>, RelayTargetResponseFactory<TResponse>>();

			return builder;
		}
	}
}
