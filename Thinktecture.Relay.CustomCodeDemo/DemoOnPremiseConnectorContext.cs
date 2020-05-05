using Serilog;
using Thinktecture.Relay.Server.Communication;

namespace Thinktecture.Relay.CustomCodeDemo
{
	internal class DemoOnPremiseConnectorContext : OnPremiseConnectionContext
	{
		public DemoOnPremiseConnectorContext(ILogger logger)
		{
			logger?.Information("Creating own connector context");
		}

		public override bool SupportsConfiguration => false;
	}
}
