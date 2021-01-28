using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
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
	/// Extension methods for the <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/>.
	/// </summary>
	public static class RelayServerBuilderExtensions
	{
		/// <summary>
		/// Adds the connector transport based on SignalR.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse, TAcknowledge> AddSignalRConnectorTransport<TRequest, TResponse, TAcknowledge>(
			this IRelayServerBuilder<TRequest, TResponse, TAcknowledge> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
			where TAcknowledge : IAcknowledgeRequest
		{
			builder.Services
				.AddTransient<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>()
				.AddTransient<IPostConfigureOptions<HubOptions<ConnectorHub<TRequest, TResponse, TAcknowledge>>>,
					HubOptionsPostConfigureOptions<TRequest, TResponse, TAcknowledge>>();

			builder.Services
				.TryAddTransient<ITenantConnectorAdapterFactory<TRequest>, TenantConnectorAdapterFactory<TRequest, TResponse, TAcknowledge>>();
			builder.Services.TryAddScoped<IConnectorTransportLimit, ConnectorHub<TRequest, TResponse, TAcknowledge>>();
			builder.Services.AddSingleton<IApplicationBuilderPart, ApplicationBuilderPart<TRequest, TResponse, TAcknowledge>>();

			builder.Services.AddSignalR();

			return builder;
		}
	}
}
