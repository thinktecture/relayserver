using System;
using System.Configuration;
using NLog.Interface;

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
        public bool UseInsecureHttp { get; private set; }
        public bool EnableManagementWeb { get; private set; }
        public bool EnableRelaying { get; private set; }
        public bool EnableOnPremiseConnections { get; private set; }
        public string HostName { get; private set; }
        public int Port { get; private set; }

        public Configuration(ILogger logger)
        {
            int tmpInt;
            bool tmpBool;

            if (!int.TryParse(ConfigurationManager.AppSettings["OnPremiseConnectorCallbackTimeout"], out tmpInt))
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
            if (int.TryParse(ConfigurationManager.AppSettings["LinkPasswordLength"], out tmpInt))
            {
                LinkPasswordLength = tmpInt;
            }

            ConnectionTimeout = 5;
            if (int.TryParse(ConfigurationManager.AppSettings["ConnectionTimeout"], out tmpInt))
            {
                ConnectionTimeout = tmpInt;
            }

            DisconnectTimeout = 6;
            if (int.TryParse(ConfigurationManager.AppSettings["DisconnectTimeout"], out tmpInt))
            {
                DisconnectTimeout = tmpInt;
            }

            HostName = ConfigurationManager.AppSettings["HostName"] ?? "+";

            Port = 20000;
            if (int.TryParse(ConfigurationManager.AppSettings["Port"], out tmpInt))
            {
                Port = tmpInt;
            }

            EnableManagementWeb = true;
            if (bool.TryParse(ConfigurationManager.AppSettings["EnableManagementWeb"], out tmpBool))
            {
                EnableManagementWeb = tmpBool;
            }

            EnableRelaying = true;
            if (bool.TryParse(ConfigurationManager.AppSettings["EnableRelaying"], out tmpBool))
            {
                EnableRelaying = tmpBool;
            }

            EnableOnPremiseConnections = true;
            if (bool.TryParse(ConfigurationManager.AppSettings["EnableOnPremiseConnections"], out tmpBool))
            {
                EnableOnPremiseConnections = tmpBool;
            }

            UseInsecureHttp = false;
            if (bool.TryParse(ConfigurationManager.AppSettings["UseInsecureHttp"], out tmpBool))
            {
                UseInsecureHttp = tmpBool;
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
        }
    }
}