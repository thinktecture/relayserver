using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public sealed class OnPremiseWebTargetElement : OnPremiseTargetElement
	{
		private readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

		private readonly ConfigurationProperty _ignoreSslErrors = new ConfigurationProperty("ignoreSslErrors", typeof(bool), false);

		public OnPremiseWebTargetElement()
		{
			Properties.Add(_baseUrl);
			Properties.Add(_ignoreSslErrors);
		}

		public string BaseUrl => (string)this[_baseUrl];

		public bool IgnoreSslErrors => (bool?)this[_ignoreSslErrors] ?? false;
	}
}
