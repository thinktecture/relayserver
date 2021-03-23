using System.Net.Http;
using AutofacSerilogIntegration;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.OnPremiseConnector.Interceptor;
using Thinktecture.Relay.OnPremiseConnector.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.ServerMigration;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

// ReSharper disable once CheckNamespace
namespace Autofac
{
	/// <summary>
	/// Contains extension methods for the Autofac <see cref="ContainerBuilder"/>.
	/// </summary>
	public static class ContainerBuilderExtensions
	{
		/// <summary>
		/// Adds all services that are required to use the OnPremiseConnector.
		/// </summary>
		/// <param name="builder">The <see cref="ContainerBuilder"/> to add the services to.</param>
		/// <returns>The <see cref="ContainerBuilder"/> with added registrations.</returns>
		public static ContainerBuilder RegisterOnPremiseConnectorTypes(this ContainerBuilder builder)
		{
			builder.RegisterLogger();
			builder.RegisterType<OnPremiseWebTargetRequestMessageBuilder>().As<IOnPremiseWebTargetRequestMessageBuilder>();
			builder.RegisterType<RelayServerConnectionFactory>().As<IRelayServerConnectionFactory>();
			builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>().SingleInstance();
			builder.RegisterType<OnPremiseTargetConnectorFactory>().As<IOnPremiseTargetConnectorFactory>();
			builder.RegisterType<HeartbeatChecker>().As<IHeartbeatChecker>();
			builder.RegisterType<TokenExpiryChecker>().As<ITokenExpiryChecker>();
			builder.RegisterType<AutomaticDisconnectChecker>().As<IAutomaticDisconnectChecker>();
			builder.RegisterType<MaintenanceLoop>().As<IMaintenanceLoop>().SingleInstance().OnActivated(e => e.Instance.StartLoop());
			builder.RegisterType<OnPremiseInterceptorFactory>().As<IOnPremiseInterceptorFactory>();

			// Types for server migration
			builder.RegisterType<RelayServerConnection>();
			builder.RegisterType<FakeApplicationLifetime>().As<IApplicationLifetime>().SingleInstance();

			return builder;
		}
	}
}
