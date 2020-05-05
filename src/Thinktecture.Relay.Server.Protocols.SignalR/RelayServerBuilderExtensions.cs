using Microsoft.Extensions.DependencyInjection.Extensions;
using Thinktecture.Relay.Server;
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
		/// Adds the <see cref="IConnectorTransport{TRequest}"/> based on SignalR.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddSignalRConnectorTransport<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
		{
			builder.Services.TryAddSingleton<IConnectorTransport<TRequest>, ConnectorTransport<TRequest, TResponse>>();
			builder.Services.AddSignalR();

			return builder;
		}
	}
}
