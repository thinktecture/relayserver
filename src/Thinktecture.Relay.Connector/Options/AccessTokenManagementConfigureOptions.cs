using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Thinktecture.Relay.Connector.Options;

internal partial class AccessTokenManagementConfigureOptions : IConfigureOptions<AccessTokenManagementOptions>
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger _logger;
	private readonly RelayConnectorOptions _relayConnectorOptions;

	public AccessTokenManagementConfigureOptions(ILogger<AccessTokenManagementConfigureOptions> logger,
		IHostApplicationLifetime hostApplicationLifetime, IOptions<RelayConnectorOptions> relayConnectorOptions,
		IHttpClientFactory httpClientFactory)
	{
		if (relayConnectorOptions is null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_hostApplicationLifetime =
			hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		_relayConnectorOptions = relayConnectorOptions.Value;
	}

	public void Configure(AccessTokenManagementOptions options)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientNames.ConnectionClose);

		var baseUri = new Uri(_relayConnectorOptions.DiscoveryDocument.AuthorizationServer);
		var fullUri = new Uri(baseUri, OidcConstants.Discovery.DiscoveryEndpoint);
		while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
		{
			var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(fullUri.AbsoluteUri,
				new OpenIdConnectConfigurationRetriever(),
				new HttpDocumentRetriever(httpClient) { RequireHttps = fullUri.Scheme == "https" });

			try
			{
				var configuration = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
				Log.GotDiscoveryDocument(_logger, fullUri, configuration);

				options.Client.Clients.Add(Constants.HttpClientNames.RelayServer, new ClientCredentialsTokenRequest()
				{
					Address = configuration.TokenEndpoint,
					ClientId = _relayConnectorOptions.TenantName,
					ClientSecret = _relayConnectorOptions.TenantSecret,
					Scope = Constants.RelayServerScopes,
				});
				break;
			}
			catch (Exception ex)
			{
				Log.ErrorRetrievingDiscoveryDocument(_logger, ex, fullUri);

				try
				{
					Task.Delay(TimeSpan.FromSeconds(10), _hostApplicationLifetime.ApplicationStopping).GetAwaiter()
						.GetResult();
				}
				catch (OperationCanceledException)
				{
					// Ignore this, as this will be thrown when the service shuts down gracefully
				}
			}
		}
	}
}
