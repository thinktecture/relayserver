using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
    public sealed class OnPremiseInProcTargetElement : OnPremiseTargetElement
    {
        private readonly ConfigurationProperty _typeName = new ConfigurationProperty("typeName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        public OnPremiseInProcTargetElement()
            : base()
        {
            Properties.Add(_typeName);
        }

        public string TypeName
        {
            get { return (string) this[_typeName]; }
        }
    }
}