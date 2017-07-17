using System;

namespace Thinktecture.Relay.Server.Configuration
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
		bool EnableManagementWeb { get; }
		bool EnableRelaying { get; }
		bool EnableOnPremiseConnections { get; }
		string HostName { get; }
		int Port { get; }
		string ManagementWebLocation { get; }
	}
}
