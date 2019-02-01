using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public sealed class OnPremiseWebTargetElement : OnPremiseTargetElement
	{
		private readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
		private readonly ConfigurationProperty _followRedirects = new ConfigurationProperty("followRedirects", typeof(bool), true);

		public OnPremiseWebTargetElement()
		{
			Properties.Add(_baseUrl);
			Properties.Add(_followRedirects);
		}

		public string BaseUrl => (string) this[_baseUrl];
		public bool FollowRedirects => (bool) this[_followRedirects];
	}
}
