using System;
using System.Configuration;
using Serilog;

namespace Thinktecture.Relay.Server.Config
{
	internal class Configuration : IConfiguration
	{
		public TimeSpan OnPremiseConnectorCallbackTimeout { get; }
		public string RabbitMqConnectionString { get; }
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
		public int ActiveConnectionTimeoutInSeconds { get; }
		public string InterceptorAssembly { get; }
		public string OAuthSharedSecret { get; set; }
		public string OAuthCertificate { get; set; }
		public TimeSpan HstsHeaderMaxAge { get; set; }
		public bool HstsIncludeSubdomains { get; set; }

		public Configuration(ILogger logger)
		{
			if (!Int32.TryParse(ConfigurationManager.AppSettings["OnPremiseConnectorCallbackTimeout"], out var tmpInt))
			{
				tmpInt = 30;
			}

			OnPremiseConnectorCallbackTimeout = TimeSpan.FromSeconds(tmpInt);

			var settings = ConfigurationManager.ConnectionStrings["RabbitMQ"];
			if (settings != null)
			{
				RabbitMqConnectionString = settings.ConnectionString;
			}

			TraceFileDirectory = ConfigurationManager.AppSettings.Get("TraceFileDirectory") ?? "tracefiles";

			LinkPasswordLength = 100;
			if (Int32.TryParse(ConfigurationManager.AppSettings["LinkPasswordLength"], out tmpInt))
			{
				LinkPasswordLength = tmpInt;
			}

			ConnectionTimeout = 5;
			if (Int32.TryParse(ConfigurationManager.AppSettings["ConnectionTimeout"], out tmpInt))
			{
				ConnectionTimeout = tmpInt;
			}

			DisconnectTimeout = 6;
			if (Int32.TryParse(ConfigurationManager.AppSettings["DisconnectTimeout"], out tmpInt))
			{
				DisconnectTimeout = tmpInt;
			}

			KeepAliveInterval = DisconnectTimeout / 3;
			if (Int32.TryParse(ConfigurationManager.AppSettings["KeepAliveInterval"], out tmpInt))
			{
				KeepAliveInterval = tmpInt;
			}

			HostName = ConfigurationManager.AppSettings["HostName"] ?? "+";

			Port = 20000;
			if (Int32.TryParse(ConfigurationManager.AppSettings["Port"], out tmpInt))
			{
				Port = tmpInt;
			}

			EnableManagementWeb = ModuleBinding.True;
			if (Enum.TryParse(ConfigurationManager.AppSettings["EnableManagementWeb"], true, out ModuleBinding tmpModuleBinding))
			{
				EnableManagementWeb = tmpModuleBinding;
			}

			EnableRelaying = ModuleBinding.True;
			if (Enum.TryParse(ConfigurationManager.AppSettings["EnableRelaying"], true, out tmpModuleBinding))
			{
				EnableRelaying = tmpModuleBinding;
			}

			EnableOnPremiseConnections = ModuleBinding.True;
			if (Enum.TryParse(ConfigurationManager.AppSettings["EnableOnPremiseConnections"], true, out tmpModuleBinding))
			{
				EnableOnPremiseConnections = tmpModuleBinding;
			}

			UseInsecureHttp = false;
			if (Boolean.TryParse(ConfigurationManager.AppSettings["UseInsecureHttp"], out var tmpBool))
			{
				UseInsecureHttp = tmpBool;
			}

			ManagementWebLocation = ConfigurationManager.AppSettings["ManagementWebLocation"];
			if (String.IsNullOrWhiteSpace(ManagementWebLocation))
			{
				ManagementWebLocation = "ManagementWeb";
			}

			TemporaryRequestStoragePath = ConfigurationManager.AppSettings["TemporaryRequestStoragePath"];
			if (String.IsNullOrWhiteSpace(TemporaryRequestStoragePath))
			{
				TemporaryRequestStoragePath = null;
			}

			if (!TimeSpan.TryParse(ConfigurationManager.AppSettings["TemporaryRequestStoragePeriod"], out var tmpTimeSpan))
			{
				tmpTimeSpan = TimeSpan.FromSeconds(10);
			}

			TemporaryRequestStoragePeriod = tmpTimeSpan;

			ActiveConnectionTimeoutInSeconds = 120;
			if (Int32.TryParse(ConfigurationManager.AppSettings["ActiveConnectionTimeoutInSeconds"], out tmpInt))
			{
				ActiveConnectionTimeoutInSeconds = tmpInt;
			}

			InterceptorAssembly = ConfigurationManager.AppSettings["InterceptorAssembly"];
			if (String.IsNullOrWhiteSpace(InterceptorAssembly))
			{
				InterceptorAssembly = null;
			}

			OAuthSharedSecret = ConfigurationManager.AppSettings["OAuthSharedSecret"];
			OAuthCertificate = ConfigurationManager.AppSettings["OAuthCertificate"];

			if (!TimeSpan.TryParse(ConfigurationManager.AppSettings["HstsHeaderMaxAge"], out tmpTimeSpan))
			{
				tmpTimeSpan = TimeSpan.FromDays(365);
			}

			HstsHeaderMaxAge = tmpTimeSpan;

			HstsIncludeSubdomains = false;
			if (Boolean.TryParse(ConfigurationManager.AppSettings["HstsIncludeSubdomains"], out tmpBool))
			{
				HstsIncludeSubdomains = tmpBool;
			}

			LogSettings(logger);
		}

		private void LogSettings(ILogger logger)
		{
			logger?.Verbose("Setting OnPremiseConnectorCallbackTimeout: {CallbackTimeout}", OnPremiseConnectorCallbackTimeout);
			logger?.Verbose("Setting RabbitMqConnectionString: {RabbitConnectionString}", RabbitMqConnectionString);
			logger?.Verbose("Setting TraceFileDirectory: {TraceFileDirectory}", TraceFileDirectory);
			logger?.Verbose("Setting LinkPasswordLength: {LinkPasswordLength}", LinkPasswordLength);
			logger?.Verbose("Setting DisconnectTimeout: {DisconnectTimeout}", DisconnectTimeout);
			logger?.Verbose("Setting ConnectionTimeout: {ConnectionTimeout}", ConnectionTimeout);
			logger?.Verbose("Setting UseInsecureHttp: {UseInsecureHttp}", UseInsecureHttp);
			logger?.Verbose("Setting EnableManagementWeb: {EnableManagementweb}", EnableManagementWeb);
			logger?.Verbose("Setting EnableRelaying: {EnableRelay}", EnableRelaying);
			logger?.Verbose("Setting EnableOnPremiseConnections: {EnableOnpremiseConnections}", EnableOnPremiseConnections);
			logger?.Verbose("Setting HostName: {Hostname}", HostName);
			logger?.Verbose("Setting Port: {Port}", Port);
			logger?.Verbose("Setting ManagementWebLocation: {ManagementwebLocation}", ManagementWebLocation);
			logger?.Verbose("Setting TemporaryRequestStoragePath: {TempStoragePath}", TemporaryRequestStoragePath ?? "not defined - using in-memory store");
			logger?.Verbose("Setting TemporaryRequestStoragePeriod: {TempStoragePeriod}", TemporaryRequestStoragePeriod);
			logger?.Verbose("Setting ActiveConnectionTimeoutInSeconds: {ActiveConnectionTimeout}", ActiveConnectionTimeoutInSeconds);
			logger?.Verbose("Setting InterceptorAssembly: {InterceptorAssembly}", InterceptorAssembly);
			logger?.Verbose("Setting OAuthSharedSecret: {OauthSharedSecret}", OAuthSharedSecret);
			logger?.Verbose("Setting OAuthCertificate: {OauthCertificate}", OAuthCertificate);
			logger?.Verbose("Setting HstsHeaderMaxAge: {HstsHeaderMaxAge}", HstsHeaderMaxAge);
			logger?.Verbose("Setting HstsIncludeSubdomains: {HstsIncludeSubdomains}", HstsIncludeSubdomains);
		}
	}
}
