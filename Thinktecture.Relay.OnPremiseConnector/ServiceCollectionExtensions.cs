using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.OnPremiseConnector.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector
{
	/// <summary>
	/// Provides extensions for the <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds all services that are required to use the OnPremiseConnector.
		/// </summary>
		/// <param name="collection">The <see cref="IServiceCollection"/> to add the service registrations to.</param>
		/// <returns>The <see cref="IServiceCollection"/> with added registrations.</returns>
		public static IServiceCollection AddOnPremiseConnectorServices(this IServiceCollection collection)
		{
			return collection
				.AddSingleton<IOnPremiseWebTargetRequestMessageBuilder, OnPremiseWebTargetRequestMessageBuilder>()
				.AddSingleton<IHttpClientFactory, HttpClientFactory>()
				.AddSingleton<MaintenanceLoop>()
				.AddSingleton<IMaintenanceLoop>(ctx =>
				{
					var loop = ctx.GetService<MaintenanceLoop>();
					loop.StartLoop();
					return loop;
				})
				.AddTransient<IRelayServerConnectionFactory, RelayServerConnectionFactory>()
				.AddTransient<IOnPremiseTargetConnectorFactory, OnPremiseTargetConnectorFactory>()
				.AddTransient<IHeartbeatChecker, HeartbeatChecker>()
				.AddTransient<ITokenExpiryChecker, TokenExpiryChecker>();
		}

	}
}
