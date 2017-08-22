using System;
using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class IdentityElement : ConfigurationElement
	{
		private static readonly ConfigurationProperty _userName = new ConfigurationProperty("userName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
		private static readonly ConfigurationProperty _password = new ConfigurationProperty("password", typeof(string), String.Empty);

		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection()
		{
			_userName,
			_password
		};

		protected override ConfigurationPropertyCollection Properties => _properties;
		public string UserName => (string)this[_userName];
		public string Password => (string)this[_password];
	}
}
