using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
    public sealed class OnPremiseWebTargetElement : OnPremiseTargetElement
    {
        private readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        public OnPremiseWebTargetElement()
            : base()
        {
            Properties.Add(_baseUrl);
        }

        public string BaseUrl
        {
            get { return (string) this[_baseUrl]; }
        }
    }
}