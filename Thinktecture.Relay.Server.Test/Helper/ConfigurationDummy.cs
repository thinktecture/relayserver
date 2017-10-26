using System;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Helper
{
	public class ConfigurationDummy : IConfiguration
	{
		public TimeSpan OnPremiseConnectorCallbackTimeout { get; set; }
		public string RabbitMqConnectionString { get; set; }
		public string TraceFileDirectory { get; set; }
		public int LinkPasswordLength { get; set; }
		public int DisconnectTimeout { get; set; }
		public int ConnectionTimeout { get; set; }
		public int KeepAliveInterval { get; set; }
		public bool UseInsecureHttp { get; set; }
		public ModuleBinding EnableManagementWeb { get; set; }
		public ModuleBinding EnableRelaying { get; set; }
		public ModuleBinding EnableOnPremiseConnections { get; set; }
		public string HostName { get; set; }
		public int Port { get; set; }
		public string ManagementWebLocation { get; set; }
		public string TemporaryRequestStoragePath { get; set; }
		public TimeSpan TemporaryRequestStoragePeriod { get; set; }
		public int ActiveConnectionTimeoutInSeconds { get; set; }
		public string InterceptorAssembly { get; }
		public string OAuthSharedSecret { get; }
		public string OAuthCertificate { get; }

		public ConfigurationDummy()
		{
			TemporaryRequestStoragePeriod = TimeSpan.FromMinutes(1);
		}
	}
}
