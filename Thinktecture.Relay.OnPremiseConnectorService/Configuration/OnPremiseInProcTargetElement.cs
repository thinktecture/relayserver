using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public sealed class OnPremiseInProcTargetElement : OnPremiseTargetElement
	{
		private readonly ConfigurationProperty _typeName = new ConfigurationProperty("typeName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

		public OnPremiseInProcTargetElement()
		{
			Properties.Add(_typeName);
		}

		public string TypeName => (string)this[_typeName];
	}
}
