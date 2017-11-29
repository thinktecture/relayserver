using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector;
using Thinktecture.Relay.OnPremiseConnectorService.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService
{
	internal class OnPremisesService
	{
		private readonly ILogger _logger = Log.Logger;

		private RelayServerConnector _connector;

		public async Task StartAsync()
		{
			try
			{
				var section = (RelayServerSection)ConfigurationManager.GetSection("relayServer");

				if (section.OnPremiseTargets.Count == 0)
					throw new ConfigurationErrorsException("At least one on-premise target needs to be configured");

				if (section.IgnoreSslErrors)
				{
					ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
				}

				switch (section.Security.AuthenticationType)
				{
					case AuthenticationType.Identity:
						if (String.IsNullOrEmpty(section.Security.Identity.UserName))
							throw new ConfigurationErrorsException("The user name cannot be null or empty when using authentication type 'Identity'");

						_connector = new RelayServerConnector(section.Security.Identity.UserName, section.Security.Identity.Password,
							new Uri(section.BaseUrl), (int)section.RequestTimeout.TotalSeconds, (int)section.Security.AccessTokenRefreshWindow.TotalSeconds);
						break;

					case AuthenticationType.None:
						throw new ConfigurationErrorsException("Authentication type 'None' is not a valid value.");

					default:
						throw new ArgumentOutOfRangeException();
				}

				_connector.RelayedRequestHeader = "X-Relayed";

				foreach (var onPremiseTarget in section.OnPremiseTargets.OfType<OnPremiseWebTargetElement>())
				{
					_connector.RegisterOnPremiseTarget(onPremiseTarget.Key, new Uri(onPremiseTarget.BaseUrl));
				}

				foreach (var onPremiseTarget in section.OnPremiseTargets.OfType<OnPremiseInProcTargetElement>())
				{
					Type handlerType;

					var parts = onPremiseTarget.TypeName.Split(',');
					if (parts.Length == 2)
					{
						var assembly = Assembly.Load(parts[1].Trim());
						handlerType = assembly.GetType(parts[0].Trim());
					}
					else
					{
						handlerType = Type.GetType(parts[0].Trim());
					}

					if (handlerType == null)
						throw new ConfigurationErrorsException("Unknown handler type: " + onPremiseTarget.TypeName);

					if (!typeof(IOnPremiseInProcHandler).IsAssignableFrom(handlerType))
						throw new ConfigurationErrorsException("The handler type " + handlerType.Name + " does not implement the interface \"IOnPremiseInProcHandler\".");

					_connector.RegisterOnPremiseTarget(onPremiseTarget.Key, handlerType);
				}

				await _connector.ConnectAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Fatal(ex, "Fatal exception occured");
				throw;
			}
		}

		public void Stop()
		{
			if (_connector != null)
			{
				_connector.Disconnect();
				_connector.Dispose();
				_connector = null;
			}
		}
	}
}
