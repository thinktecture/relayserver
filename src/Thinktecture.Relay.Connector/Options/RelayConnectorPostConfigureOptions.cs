using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Options;

internal partial class
	RelayConnectorPostConfigureOptions<TRequest, TResponse> : IPostConfigureOptions<RelayConnectorOptions>
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime;
	private readonly ILogger _logger;
	private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
	private readonly IHttpClientFactory _httpClientFactory;

	public RelayConnectorPostConfigureOptions(ILogger<RelayConnectorPostConfigureOptions<TRequest, TResponse>> logger,
		RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry, IHostApplicationLifetime hostApplicationLifetime,
		IHttpClientFactory httpClientFactory)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
		_hostApplicationLifetime =
			hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
	}

	public void PostConfigure(string? name, RelayConnectorOptions options)
	{
		if (options.Targets is not null)
		{
			RegisterTargets(options.Targets);
		}

		var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientNames.ConnectionClose);

		var fullUri = new Uri(options.RelayServerBaseUri, DiscoveryDocument.WellKnownPath);
		while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
		{
			var configManager = new ConfigurationManager<DiscoveryDocument>(fullUri.AbsoluteUri,
				new RelayServerConfigurationRetriever(),
				new HttpDocumentRetriever(httpClient) { RequireHttps = fullUri.Scheme == "https" });

			try
			{
				options.DiscoveryDocument = configManager.GetConfigurationAsync().GetAwaiter().GetResult();
				Log.GotDiscoveryDocument(_logger, fullUri, options.DiscoveryDocument);

				break;
			}
			catch (Exception ex)
			{
				Log.ErrorRetrievingDiscoveryDocument(_logger, ex, fullUri.AbsoluteUri);

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

	private void RegisterTargets(Dictionary<string, Dictionary<string, string>> targets)
	{
		foreach (var (key, value) in targets)
		{
			var parameters = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);

			var typeName = parameters[Constants.RelayConnectorOptionsTargetType];
			var type = Type.GetType(typeName);
			if (type is null)
			{
				Log.ErrorTargetTypeNotFound(_logger, typeName, key);
				return;
			}

			TimeSpan? timeout = null;
			if (parameters.TryGetValue(Constants.RelayConnectorOptionsTargetTimeout, out var timeoutParameter))
			{
				if (TimeSpan.TryParse(timeoutParameter, out var timeoutValue))
				{
					timeout = timeoutValue;
				}
				else
				{
					Log.CouldNotParseTimeout(_logger, timeoutParameter, key);
				}
			}

			parameters.Add(Constants.RelayConnectorOptionsTargetId, key);
			parameters.Remove(Constants.RelayConnectorOptionsTargetType);
			parameters.Remove(Constants.RelayConnectorOptionsTargetTimeout);

			_relayTargetRegistry.Register(key, type, timeout, parameters);
		}
	}
}
