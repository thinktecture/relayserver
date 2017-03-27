﻿using System;
using System.ComponentModel;
using System.Configuration;

namespace Thinktecture.Relay.OnPremiseConnectorService.Configuration
{
	public class RelayServerSection : ConfigurationSection
	{
		private static readonly ConfigurationProperty _baseUrl = new ConfigurationProperty("baseUrl", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
		private static readonly ConfigurationProperty _requestTimeout = new ConfigurationProperty("timeout", typeof(TimeSpan), "00:00:10", new TimeSpanConverter(), new PositiveTimeSpanValidator(), ConfigurationPropertyOptions.None);
		private static readonly ConfigurationProperty _security = new ConfigurationProperty("security", typeof(SecurityElement), null, ConfigurationPropertyOptions.IsRequired);
		private static readonly ConfigurationProperty _onPremiseTargets = new ConfigurationProperty("onPremiseTargets", typeof(OnPremiseTargetCollection), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _maxRetries = new ConfigurationProperty("maxRetries", typeof(int), 3, ConfigurationPropertyOptions.None);

		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection
		{
			_baseUrl,
			_requestTimeout,
			_security,
			_onPremiseTargets,
            _maxRetries
		};

		protected override ConfigurationPropertyCollection Properties => _properties;

	    public string BaseUrl => (string) this[_baseUrl];

	    public TimeSpan RequestTimeout => (TimeSpan) this[_requestTimeout];

	    public SecurityElement Security => (SecurityElement) this[_security];

	    public OnPremiseTargetCollection OnPremiseTargets => (OnPremiseTargetCollection) this[_onPremiseTargets];

	    public int MaxRetries => (int) this[_maxRetries];
	}
}
