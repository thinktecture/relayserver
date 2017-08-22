using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public abstract class OnPremiseTargetElement : ConfigurationElement
	{
		private readonly ConfigurationProperty _key = new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);

		protected OnPremiseTargetElement()
		{
			Properties = new ConfigurationPropertyCollection() { _key };
		}

		protected override ConfigurationPropertyCollection Properties { get; }

		public string Key => (string)this[_key];
	}
}
