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

internal class RelayConnectorPostConfigureOptions<TRequest, TResponse> : IPostConfigureOptions<RelayConnectorOptions>
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime;
	private readonly ILogger<RelayConnectorPostConfigureOptions<TRequest, TResponse>> _logger;
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

	public void PostConfigure(string name, RelayConnectorOptions options)
	{
		if (options.Targets.Count > 0)
		{
			RegisterTargets(options.Targets);
		}

		var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientNames.ConnectionClose);
		httpClient.BaseAddress = options.RelayServerBaseUri;

		var uri = httpClient.BaseAddress + DiscoveryDocument.WellKnownPath;

		while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
		{
			var configManager = new ConfigurationManager<DiscoveryDocument>(DiscoveryDocument.WellKnownPath,
				new RelayServerConfigurationRetriever(),
				new HttpDocumentRetriever(httpClient) { RequireHttps = httpClient.BaseAddress?.Scheme == "https" });

			try
			{
				options.DiscoveryDocument = configManager.GetConfigurationAsync().GetAwaiter().GetResult();
				_logger.LogTrace(10300, "Got discovery document from {DiscoveryDocumentUrl} ({@DiscoveryDocument})", uri,
					options.DiscoveryDocument);

				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(10301, ex,
					"An error occured while retrieving the discovery document from {DiscoveryDocumentUrl}", uri);

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
			if (type == null)
			{
				_logger.LogError(10302, "Could not find target type {TargetType}", typeName);
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
					_logger.LogWarning(10303, "Could not parse timeout \"{TargetTimeout}\" for target {Target}",
						timeoutParameter,
						key);
				}
			}

			parameters.Add(Constants.RelayConnectorOptionsTargetId, key);
			parameters.Remove(Constants.RelayConnectorOptionsTargetType);
			parameters.Remove(Constants.RelayConnectorOptionsTargetTimeout);

			_relayTargetRegistry.Register(key, type, timeout, parameters);
		}
	}
}
