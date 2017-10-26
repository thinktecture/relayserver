using Autofac;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal interface IInterceptorLoader
	{
		void LoadInterceptors(ContainerBuilder builder);
	}
}
