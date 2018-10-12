using System;
using System.Configuration;
using System.IO;
using System.Web.Http;
using Serilog;

namespace Thinktecture.Relay.Server.Config
{
	internal class Configuration : IConfiguration
	{
		//  RabbitMQ Settings
		public string RabbitMqConnectionString { get; }
		public string RabbitMqClusterHosts { get; }
		public TimeSpan QueueExpiration { get; }
		public TimeSpan RequestExpiration { get; }

		// App Settings
		public TimeSpan OnPremiseConnectorCallbackTimeout { get; }
		public string TraceFileDirectory { get; }
		public int LinkPasswordLength { get; }
		public int DisconnectTimeout { get; }
		public int ConnectionTimeout { get; }
		public int KeepAliveInterval { get; }
		public bool UseInsecureHttp { get; }
		public ModuleBinding EnableManagementWeb { get; }
		public ModuleBinding EnableRelaying { get; }
		public ModuleBinding EnableOnPremiseConnections { get; }
		public string HostName { get; }
		public int Port { get; }
		public string ManagementWebLocation { get; }
		public TimeSpan TemporaryRequestStoragePeriod { get; }
		public string TemporaryRequestStoragePath { get; }
		public TimeSpan ActiveConnectionTimeout { get; }
		public string CustomCodeAssemblyPath { get; set; }
		public string SharedSecret { get; }
		public string OAuthCertificate { get; }
		public TimeSpan HstsHeaderMaxAge { get; }
		public bool HstsIncludeSubdomains { get; }
		public IncludeErrorDetailPolicy IncludeErrorDetailPolicy { get; }
		public int MaxFailedLoginAttempts { get; }
		public TimeSpan FailedLoginLockoutPeriod { get; }
		public bool SecureClientController { get; }
		public TimeSpan AccessTokenLifetime { get; }

		public Configuration(ILogger logger)
		{
			var settings = ConfigurationManager.ConnectionStrings["RabbitMQ"];
			if (settings != null)
			{
				RabbitMqConnectionString = settings.ConnectionString;
			}

			RabbitMqClusterHosts = ConfigurationManager.AppSettings[nameof(RabbitMqClusterHosts)];
			if (String.IsNullOrWhiteSpace(RabbitMqClusterHosts))
			{
				RabbitMqClusterHosts = null;
			}

			QueueExpiration = TimeSpan.FromSeconds(10);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(QueueExpiration)], out var tmpTimeSpan))
			{
				QueueExpiration = tmpTimeSpan;
			}

			RequestExpiration = TimeSpan.FromSeconds(10);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(RequestExpiration)], out tmpTimeSpan))
			{
				RequestExpiration = tmpTimeSpan;
			}

			OnPremiseConnectorCallbackTimeout = TimeSpan.FromSeconds(30);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(OnPremiseConnectorCallbackTimeout)], out tmpTimeSpan))
			{
				OnPremiseConnectorCallbackTimeout = tmpTimeSpan;
			}

			TraceFileDirectory = ConfigurationManager.AppSettings[nameof(TraceFileDirectory)] ?? "tracefiles";

			LinkPasswordLength = 100;
			if (Int32.TryParse(ConfigurationManager.AppSettings[nameof(LinkPasswordLength)], out var tmpInt))
			{
				LinkPasswordLength = tmpInt;
			}

			DisconnectTimeout = 6;
			if (Int32.TryParse(ConfigurationManager.AppSettings[nameof(DisconnectTimeout)], out tmpInt))
			{
				DisconnectTimeout = tmpInt;
			}

			ConnectionTimeout = 5;
			if (Int32.TryParse(ConfigurationManager.AppSettings[nameof(ConnectionTimeout)], out tmpInt))
			{
				ConnectionTimeout = tmpInt;
			}

			KeepAliveInterval = DisconnectTimeout / 3;
			if (Int32.TryParse(ConfigurationManager.AppSettings[nameof(KeepAliveInterval)], out tmpInt) && tmpInt >= KeepAliveInterval)
			{
				KeepAliveInterval = tmpInt;
			}

			UseInsecureHttp = false;
			if (Boolean.TryParse(ConfigurationManager.AppSettings[nameof(UseInsecureHttp)], out var tmpBool))
			{
				UseInsecureHttp = tmpBool;
			}

			EnableManagementWeb = ModuleBinding.True;
			if (Enum.TryParse(ConfigurationManager.AppSettings[nameof(EnableManagementWeb)], true, out ModuleBinding tmpModuleBinding))
			{
				EnableManagementWeb = tmpModuleBinding;
			}

			EnableRelaying = ModuleBinding.True;
			if (Enum.TryParse(ConfigurationManager.AppSettings[nameof(EnableRelaying)], true, out tmpModuleBinding))
			{
				EnableRelaying = tmpModuleBinding;
			}

			EnableOnPremiseConnections = ModuleBinding.True;
			if (Enum.TryParse(ConfigurationManager.AppSettings[nameof(EnableOnPremiseConnections)], true, out tmpModuleBinding))
			{
				EnableOnPremiseConnections = tmpModuleBinding;
			}

			HostName = ConfigurationManager.AppSettings[nameof(HostName)] ?? "+";

			Port = UseInsecureHttp ? 20000 : 443;
			if (Int32.TryParse(ConfigurationManager.AppSettings[nameof(Port)], out tmpInt))
			{
				Port = tmpInt;
			}

			ManagementWebLocation = ConfigurationManager.AppSettings[nameof(ManagementWebLocation)];
			if (String.IsNullOrWhiteSpace(ManagementWebLocation))
			{
				ManagementWebLocation = "ManagementWeb";
			}

			TemporaryRequestStoragePath = ConfigurationManager.AppSettings[nameof(TemporaryRequestStoragePath)];
			if (String.IsNullOrWhiteSpace(TemporaryRequestStoragePath))
			{
				TemporaryRequestStoragePath = null;
			}

			TemporaryRequestStoragePeriod = OnPremiseConnectorCallbackTimeout + OnPremiseConnectorCallbackTimeout;
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(TemporaryRequestStoragePeriod)], out tmpTimeSpan) && tmpTimeSpan >= TemporaryRequestStoragePeriod)
			{
				TemporaryRequestStoragePeriod = tmpTimeSpan;
			}

			ActiveConnectionTimeout = TimeSpan.FromMinutes(2);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(ActiveConnectionTimeout)], out tmpTimeSpan))
			{
				ActiveConnectionTimeout = tmpTimeSpan;
			}

			CustomCodeAssemblyPath = ConfigurationManager.AppSettings[nameof(CustomCodeAssemblyPath)];
			if (String.IsNullOrWhiteSpace(CustomCodeAssemblyPath))
			{
				CustomCodeAssemblyPath = null;
			}
			else if (!File.Exists(CustomCodeAssemblyPath))
			{
				logger?.Warning("A custom code assembly has been configured, but it is not available at the configured path. assembly-path={CustomCodeAssemblyPath}", CustomCodeAssemblyPath);
				CustomCodeAssemblyPath = null;
			}

			SharedSecret = ConfigurationManager.AppSettings[nameof(SharedSecret)];
			OAuthCertificate = ConfigurationManager.AppSettings[nameof(OAuthCertificate)];

			HstsHeaderMaxAge = TimeSpan.FromDays(365);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(HstsHeaderMaxAge)], out tmpTimeSpan))
			{
				HstsHeaderMaxAge = tmpTimeSpan;
			}

			HstsIncludeSubdomains = false;
			if (Boolean.TryParse(ConfigurationManager.AppSettings[nameof(HstsIncludeSubdomains)], out tmpBool))
			{
				HstsIncludeSubdomains = tmpBool;
			}

			IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;
			if (Enum.TryParse(ConfigurationManager.AppSettings[nameof(IncludeErrorDetailPolicy)], true, out IncludeErrorDetailPolicy tmpIncludeErrorDetailPolicy))
			{
				IncludeErrorDetailPolicy = tmpIncludeErrorDetailPolicy;
			}

			MaxFailedLoginAttempts = 5;
			if (Int32.TryParse(ConfigurationManager.AppSettings[nameof(MaxFailedLoginAttempts)], out tmpInt))
			{
				MaxFailedLoginAttempts = tmpInt;
			}

			FailedLoginLockoutPeriod = TimeSpan.FromMinutes(15);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(FailedLoginLockoutPeriod)], out tmpTimeSpan))
			{
				FailedLoginLockoutPeriod = tmpTimeSpan;
			}

			SecureClientController = false;
			if (Boolean.TryParse(ConfigurationManager.AppSettings[nameof(SecureClientController)], out tmpBool))
			{
				SecureClientController = tmpBool;
			}

			AccessTokenLifetime = TimeSpan.FromDays(365);
			if (TimeSpan.TryParse(ConfigurationManager.AppSettings[nameof(AccessTokenLifetime)], out tmpTimeSpan))
			{
				AccessTokenLifetime = tmpTimeSpan;
			}

			LogSettings(logger);
		}

		private void LogSettings(ILogger logger)
		{
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(RabbitMqConnectionString), RabbitMqConnectionString);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(RabbitMqClusterHosts), RabbitMqClusterHosts ?? "not defined - using single host");
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(QueueExpiration), QueueExpiration);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(RequestExpiration), RequestExpiration);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(OnPremiseConnectorCallbackTimeout), OnPremiseConnectorCallbackTimeout);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(TraceFileDirectory), TraceFileDirectory);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(LinkPasswordLength), LinkPasswordLength);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(DisconnectTimeout), DisconnectTimeout);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(ConnectionTimeout), ConnectionTimeout);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(KeepAliveInterval), KeepAliveInterval);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(UseInsecureHttp), UseInsecureHttp);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(EnableManagementWeb), EnableManagementWeb);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(EnableRelaying), EnableRelaying);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(EnableOnPremiseConnections), EnableOnPremiseConnections);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(HostName), HostName);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(Port), Port);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(ManagementWebLocation), ManagementWebLocation);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(TemporaryRequestStoragePath), TemporaryRequestStoragePath ?? "not defined - using in-memory store");
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(TemporaryRequestStoragePeriod), TemporaryRequestStoragePeriod);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(ActiveConnectionTimeout), ActiveConnectionTimeout);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(CustomCodeAssemblyPath), CustomCodeAssemblyPath);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(SharedSecret), SharedSecret);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(OAuthCertificate), OAuthCertificate);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(HstsHeaderMaxAge), HstsHeaderMaxAge);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(HstsIncludeSubdomains), HstsIncludeSubdomains);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(IncludeErrorDetailPolicy), IncludeErrorDetailPolicy);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(MaxFailedLoginAttempts), MaxFailedLoginAttempts);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(FailedLoginLockoutPeriod), FailedLoginLockoutPeriod);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(SecureClientController), SecureClientController);
			logger?.Verbose("Setting {ConfigurationProperty}: {ConfigurationValue}", nameof(AccessTokenLifetime), AccessTokenLifetime);
		}
	}
}
