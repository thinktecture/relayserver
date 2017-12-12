using Autofac;

namespace Thinktecture.Relay.Server.Controller
{
	internal interface IControllerLoader
	{
		void RegisterControllers(ContainerBuilder builder);
	}
}