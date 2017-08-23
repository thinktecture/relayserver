using Autofac;
using Thinktecture.Relay.Server.Autofac;

namespace Thinktecture.Relay.Server.Controller
{
	public class RelayingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ClientController>().InjectPropertiesAsAutowired();

			base.Load(builder);
		}
	}
}
