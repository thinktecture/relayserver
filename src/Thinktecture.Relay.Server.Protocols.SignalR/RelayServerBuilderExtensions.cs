using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Protocols.SignalR;
using Thinktecture.Relay.Server.Protocols.SignalR.Options;
using Thinktecture.Relay.Transport;
using JwtBearerPostConfigureOptions = Thinktecture.Relay.Server.Protocols.SignalR.Options.JwtBearerPostConfigureOptions;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
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
			where TResponse : class, ITargetResponse
		{
			builder.Services
				.AddTransient<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>()
				.AddTransient<IPostConfigureOptions<HubOptions<ConnectorHub<TRequest, TResponse>>>,
					HubOptionsPostConfigureOptions<TRequest, TResponse>>();

			builder.Services.TryAddTransient<ITenantConnectorAdapterFactory<TRequest>, TenantConnectorAdapterFactory<TRequest, TResponse>>();
			builder.Services.TryAddScoped<IConnectorTransport<TResponse>, ConnectorHub<TRequest, TResponse>>();
			builder.Services.AddSingleton<IApplicationBuilderPart, ApplicationBuilderPart<TRequest, TResponse>>();

			builder.Services.AddSignalR();

			return builder;
		}
	}
}
