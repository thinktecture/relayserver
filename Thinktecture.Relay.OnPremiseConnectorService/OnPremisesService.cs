using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.OnPremiseConnector;
using Thinktecture.Relay.OnPremiseConnectorService.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService
{
	internal class OnPremisesService
	{
		private RelayServerConnector _connector;
	    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public async Task Start()
		{
		    try
		    {
		        var section = (RelayServerSection) ConfigurationManager.GetSection("relayServer");

		        if (section.OnPremiseTargets.Count == 0)
		            throw new ConfigurationErrorsException("At least one On-Premise Target needs to be configured.");

		        switch (section.Security.AuthenticationType)
		        {
		            case AuthenticationType.Identity:
		                if (string.IsNullOrEmpty(section.Security.Identity.UserName))
		                    throw new ConfigurationErrorsException(
		                        "The user name cannot be null or empty when using authentication type 'Identity'.");

		                _connector = new RelayServerConnector(section.Security.Identity.UserName,
		                    section.Security.Identity.Password, new Uri(section.BaseUrl),
		                    (int) section.RequestTimeout.TotalSeconds);
		                break;

		            default:
		                throw new ArgumentOutOfRangeException();
		        }

		        _connector.RelayedRequestHeader = "X-Relayed";

		        foreach (var onPremiseTarget in section.OnPremiseTargets.Cast<OnPremiseTargetElement>())
		        {
		            _connector.RegisterOnPremiseTarget(onPremiseTarget.Key, new Uri(onPremiseTarget.BaseUrl));
		        }


		        await _connector.Connect();
		    }
		    catch (Exception e)
		    {
		        _logger.FatalException("Fatal exception occured", e);
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
