using Autofac;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
    public class ManagementWebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(RelayingModule).Assembly)
                .Where(t => t.Namespace != null && t.Namespace.EndsWith("ManagementWeb"));

            base.Load(builder);
        }
    }
}