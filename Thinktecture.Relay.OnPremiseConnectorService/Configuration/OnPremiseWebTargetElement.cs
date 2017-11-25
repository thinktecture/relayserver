using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public sealed class OnPremiseWebTargetElement : OnPremiseTargetElement
	{
		private readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
		private readonly ConfigurationProperty _relayRedirects = new ConfigurationProperty("relayRedirects", typeof(bool), false);

		public OnPremiseWebTargetElement()
		{
			Properties.Add(_baseUrl);
			Properties.Add(_relayRedirects);
		}

		public string BaseUrl => (string)this[_baseUrl];
		public bool RelayRedirects => (bool)this[_relayRedirects];
	}
}
