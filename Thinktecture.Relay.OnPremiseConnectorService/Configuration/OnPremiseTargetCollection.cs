using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class OnPremiseTargetCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new OnPremiseTargetElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((OnPremiseTargetElement) element).Key;
		}
	}
}
