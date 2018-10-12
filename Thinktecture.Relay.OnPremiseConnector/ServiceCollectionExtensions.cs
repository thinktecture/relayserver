using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.OnPremiseConnector.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector
{
	public static class ServiceCollectionExtensions
	{
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
