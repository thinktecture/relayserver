using System;
using System.Configuration;
using System.Linq;
using Topshelf;

namespace Thinktecture.Relay.Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // TODO: Use configuration (see RELAY-68)
            int port;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["Port"], out port))
            {
                port = 20000;
            }

            var hostName = ConfigurationManager.AppSettings["HostName"] ?? "+";
            var allowHttp = String.Equals(ConfigurationManager.AppSettings["UseInsecureHttp"], "true",
                StringComparison.OrdinalIgnoreCase);

            HostFactory.Run(config =>
            {
                config.Service<RelayService>(settings =>
                {
                    settings.ConstructUsing(_ => new RelayService(hostName, port, allowHttp));
                    settings.WhenStarted(s => s.Start());
                    settings.WhenStopped(s => s.Stop());
                });
                config.RunAsNetworkService();

                config.SetDescription("Thinktecture Relay Server Process");
                config.SetDisplayName("Thinktecture Relay Server");
                config.SetServiceName("TTRelayServer");
                config.ApplyCommandLine();
            });
        }
    }
}
