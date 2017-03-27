﻿using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class OnPremiseTargetElement : ConfigurationElement
	{
		private readonly ConfigurationProperty _key = new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
		private readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

		private readonly ConfigurationPropertyCollection _properties;

		public OnPremiseTargetElement()
		{
			_properties = new ConfigurationPropertyCollection
			{
				_key,
				_baseUrl
			};
		}

		protected override ConfigurationPropertyCollection Properties => _properties;

	    public string Key => (string) this[_key];

	    public string BaseUrl => (string) this[_baseUrl];
	}
}
