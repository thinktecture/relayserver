using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Protocols.SignalR;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on ServiceCollector namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayServerBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayServerBuilderExtensions
	{
		/// <summary>
		/// Adds the connector transport based on SignalR.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddSignalRConnectorTransport<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
		{
			builder.Services.TryAddSingleton<ITenantConnectorAdapterFactory<TRequest>, TenantConnectorAdapterFactory<TRequest, TResponse>>();
			builder.Services.TryAddSingleton<IConnectorTransport<TResponse>, ConnectorHub<TRequest, TResponse>>();
			builder.Services.AddSingleton<IApplicationBuilderPart, ApplicationBuilderPart<TRequest, TResponse>>();
			builder.Services.AddSignalR();

			return builder;
		}
	}
}
