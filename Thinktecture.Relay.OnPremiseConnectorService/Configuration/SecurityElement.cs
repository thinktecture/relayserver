using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class SecurityElement : ConfigurationElement
	{
		private static readonly ConfigurationProperty _authenticationType = new ConfigurationProperty("authenticationType", typeof(AuthenticationType), AuthenticationType.None, ConfigurationPropertyOptions.IsRequired);
		private static readonly ConfigurationProperty _identity = new ConfigurationProperty("identity", typeof(IdentityElement));

		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection
		{
			_authenticationType,
			_identity
		};

		protected override ConfigurationPropertyCollection Properties => _properties;

	    public AuthenticationType AuthenticationType => (AuthenticationType) this[_authenticationType];

	    public IdentityElement Identity => (IdentityElement) this[_identity];
	}
}
