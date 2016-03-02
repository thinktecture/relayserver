using Autofac;

namespace Thinktecture.Relay.Server.Controller
{
    public class OnPremiseConnectionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RequestController>();
            builder.RegisterType<ResponseController>();

            base.Load(builder);
        }
    }
}