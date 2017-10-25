using Autofac;

namespace Thinktecture.Relay.Server.Interceptors
{
	internal interface IInterceptorLoader
	{
		void LoadInterceptors(ContainerBuilder builder);
	}
}
