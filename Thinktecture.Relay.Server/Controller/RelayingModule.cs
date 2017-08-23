using Autofac;

namespace Thinktecture.Relay.Server.Controller
{
	public class RelayingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ClientController>();

			base.Load(builder);
		}
	}
}
