using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorPostConfigureOptions<TRequest, TResponse> : IPostConfigureOptions<RelayConnectorOptions>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RelayConnectorPostConfigureOptions<TRequest, TResponse>> _logger;
		private readonly RelayTargetRegistry<TRequest, TResponse> _relayTargetRegistry;
		private readonly IApplicationLifetime _applicationLifetime;

		public RelayConnectorPostConfigureOptions(ILogger<RelayConnectorPostConfigureOptions<TRequest, TResponse>> logger,
			RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry, IApplicationLifetime applicationLifetime)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_relayTargetRegistry = relayTargetRegistry ?? throw new ArgumentNullException(nameof(relayTargetRegistry));
			_applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
		}

		public void PostConfigure(string name, RelayConnectorOptions options)
		{
			if (options.Targets?.Count > 0)
			{
				RegisterTargets(options.Targets);
			}

			var uri = new Uri(options.RelayServerBaseUri, DiscoveryDocument.WellKnownPath);

			while (!_applicationLifetime.ApplicationStopping.IsCancellationRequested)
			{
				var configManager = new ConfigurationManager<DiscoveryDocument>(uri.ToString(), new RelayServerConfigurationRetriever(),
					new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https" });

				try
				{
					options.DiscoveryDocument = configManager.GetConfigurationAsync().GetAwaiter().GetResult();
					_logger.LogTrace("Got discovery document from {DiscoveryDocumentUrl} ({@DiscoveryDocument})", uri,
						options.DiscoveryDocument);

					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "An error occured while retrieving the discovery document from {DiscoverDocumentUrl}", uri);

					try
					{
						Task.Delay(TimeSpan.FromSeconds(10), _applicationLifetime.ApplicationStopping).GetAwaiter().GetResult();
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
			foreach (var kvp in targets)
			{
				var parameters = new Dictionary<string, string>(kvp.Value, StringComparer.OrdinalIgnoreCase);

				var type = Type.GetType(parameters[Constants.RelayConnectorOptionsTargetType]);
				TimeSpan? timeout = null;

				if (parameters.TryGetValue(Constants.RelayConnectorOptionsTargetTimeout, out var timeoutParameter))
				{
					if (TimeSpan.TryParse(timeoutParameter, out var timeoutValue))
					{
						timeout = timeoutValue;
					}
					else
					{
						_logger.LogWarning("Could not parse timeout \"{TargetTimeout}\" for target {Target}", timeoutParameter, kvp.Key);
					}
				}

				parameters.Add(Constants.RelayConnectorOptionsTargetId, kvp.Key);
				parameters.Remove(Constants.RelayConnectorOptionsTargetType);
				parameters.Remove(Constants.RelayConnectorOptionsTargetTimeout);

				_relayTargetRegistry.Register(kvp.Key, type, timeout, parameters);
			}
		}
	}
}
