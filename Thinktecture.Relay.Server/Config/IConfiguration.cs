using System;
using System.Web.Http;

namespace Thinktecture.Relay.Server.Config
{
	public interface IConfiguration
	{
		string RabbitMqConnectionString { get; }
		TimeSpan OnPremiseConnectorCallbackTimeout { get; }
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
		TimeSpan TemporaryRequestStoragePeriod { get; }
		string TemporaryRequestStoragePath { get; }
		TimeSpan ActiveConnectionTimeout { get; }
		string CustomCodeAssemblyPath { get; set; }
		string OAuthSharedSecret { get; }
		string OAuthCertificate { get; }
		TimeSpan HstsHeaderMaxAge { get; }
		bool HstsIncludeSubdomains { get; }
		IncludeErrorDetailPolicy IncludeErrorDetailPolicy { get; }
		int MaxFailedLoginAttempts { get; }
		TimeSpan FailedLoginLockoutPeriod { get; }
		bool SecureClientController { get; }
		TimeSpan QueueExpiration { get; }
		TimeSpan RequestExpiration { get; }
		TimeSpan AccessTokenLifetime { get; }
	}
}
