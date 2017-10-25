using Topshelf;

namespace Thinktecture.Relay.OnPremiseConnectorService
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			HostFactory.Run(config =>
			{
				config.Service<OnPremisesService>(settings =>
				{
					settings.ConstructUsing(_ => new OnPremisesService());
					settings.WhenStarted(async s => await s.StartAsync().ConfigureAwait(false));
					settings.WhenStopped(s => s.Stop());
				});
				config.RunAsNetworkService();

				config.SetDescription("Thinktecture Relay OnPremises Service");
				config.SetDisplayName("Thinktecture Relay OnPremises Service");
				config.SetServiceName("TTRelayOnPremisesService");
			});
		}
	}
}
