using System;
using System.Web.Http;

namespace Thinktecture.Relay.Server.Config
{
	public interface IConfiguration
	{
		TimeSpan OnPremiseConnectorCallbackTimeout { get; }
		string RabbitMqConnectionString { get; }
		string TraceFileDirectory { get; }
		int LinkPasswordLength { get; }
		int DisconnectTimeout { get; }
		int ConnectionTimeout { get; }
		int KeepAliveInterval { get; }
		bool UseInsecureHttp { get; }
		ModuleBinding EnableManagementWeb { get; }
		ModuleBinding EnableRelaying { get; }
		ModuleBinding EnableOnPremiseConnections { get; }
		string HostName { get; }
		int Port { get; }
		string ManagementWebLocation { get; }
		string TemporaryRequestStoragePath { get; }
		TimeSpan TemporaryRequestStoragePeriod { get; }
		int ActiveConnectionTimeoutInSeconds { get; }
		string InterceptorAssembly { get; }
		string OAuthSharedSecret { get; }
		string OAuthCertificate { get; }
		TimeSpan AccessTokenLifetime { get; set; }
		TimeSpan HstsHeaderMaxAge { get; set; }
		bool HstsIncludeSubdomains { get; set; }
		IncludeErrorDetailPolicy IncludeErrorDetailPolicy { get; set; }
	}
}
