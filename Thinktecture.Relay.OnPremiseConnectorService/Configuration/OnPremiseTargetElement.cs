using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
    public abstract class OnPremiseTargetElement : ConfigurationElement
    {
        private readonly ConfigurationProperty _key = new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);

        private readonly ConfigurationPropertyCollection _properties;

        protected OnPremiseTargetElement()
        {
            _properties = new ConfigurationPropertyCollection()
            {
                _key
            };
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        public string Key
        {
            get { return (string) this[_key]; }
        }
    }
}
