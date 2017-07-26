using System;
using System.Configuration;
using NLog;

namespace Thinktecture.Relay.Server.Configuration
{
	internal class Configuration : IConfiguration
	{
		public TimeSpan OnPremiseConnectorCallbackTimeout { get; private set; }
		public string RabbitMqConnectionString { get; private set; }
		public string TraceFileDirectory { get; private set; }
		public int LinkPasswordLength { get; private set; }
		public int DisconnectTimeout { get; private set; }
		public int ConnectionTimeout { get; private set; }
		public int KeepAliveInterval { get; private set; }
		public bool UseInsecureHttp { get; private set; }
		public ModuleBinding EnableManagementWeb { get; private set; }
		public ModuleBinding EnableRelaying { get; private set; }
		public ModuleBinding EnableOnPremiseConnections { get; private set; }
		public string HostName { get; private set; }
		public int Port { get; private set; }
		public string ManagementWebLocation { get; private set; }

		public Configuration(ILogger logger)
		{
			int tmpInt;
			bool tmpBool;
			ModuleBinding tmpModuleBinding;

			if (!Int32.TryParse(ConfigurationManager.AppSettings["OnPremiseConnectorCallbackTimeout"], out tmpInt))
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
			if (Enum.TryParse(ConfigurationManager.AppSettings["EnableManagementWeb"], true, out tmpModuleBinding))
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
			if (Boolean.TryParse(ConfigurationManager.AppSettings["UseInsecureHttp"], out tmpBool))
			{
				UseInsecureHttp = tmpBool;
			}

			ManagementWebLocation = ConfigurationManager.AppSettings["ManagementWebLocation"];
			if (String.IsNullOrWhiteSpace(ManagementWebLocation))
			{
				ManagementWebLocation = "ManagementWeb";
			}

			LogSettings(logger);
		}

		private void LogSettings(ILogger logger)
		{
			logger.Trace("Setting OnPremiseConnectorCallbackTimeout: {0}", OnPremiseConnectorCallbackTimeout);
			logger.Trace("Setting RabbitMqConnectionString: {0}", RabbitMqConnectionString);
			logger.Trace("Setting TraceFileDirectory: {0}", TraceFileDirectory);
			logger.Trace("Setting LinkPasswordLength: {0}", LinkPasswordLength);
			logger.Trace("Setting DisconnectTimeout: {0}", DisconnectTimeout);
			logger.Trace("Setting ConnectionTimeout: {0}", ConnectionTimeout);
			logger.Trace("Setting UseInsecureHttp: {0}", UseInsecureHttp);
			logger.Trace("Setting EnableManagementWeb: {0}", EnableManagementWeb);
			logger.Trace("Setting EnableRelaying: {0}", EnableRelaying);
			logger.Trace("Setting EnableOnPremiseConnections: {0}", EnableOnPremiseConnections);
			logger.Trace("Setting HostName: {0}", HostName);
			logger.Trace("Setting Port: {0}", Port);
			logger.Trace("Setting ManagementWebLocation: {0}", ManagementWebLocation);
		}
	}
}
