using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public sealed class OnPremiseWebTargetElement : OnPremiseTargetElement
	{
		private readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

		public OnPremiseWebTargetElement()
		{
			Properties.Add(_baseUrl);
		}

		public string BaseUrl => (string)this[_baseUrl];
	}
}
