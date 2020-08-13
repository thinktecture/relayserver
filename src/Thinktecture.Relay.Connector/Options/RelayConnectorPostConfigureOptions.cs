using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorPostConfigureOptions<TRequest, TResponse>
		: IPostConfigureOptions<RelayConnectorOptions<TRequest, TResponse>>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IServiceProvider _serviceProvider;

		public RelayConnectorPostConfigureOptions(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void PostConfigure(string name, RelayConnectorOptions<TRequest, TResponse> options)
		{
			var uri = new Uri(options.RelayServerBaseUri, Constants.WellKnownDiscoveryDocumentPath);

			options.ConfigurationManager = new ConfigurationManager<DiscoveryDocument>(
				uri.ToString(),
				ActivatorUtilities.CreateInstance<RelayServerConfigurationRetriever>(_serviceProvider),
				new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https", }
			);
		}
	}
}
