using Autofac;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal interface IInterceptorLoader
	{
		void RegisterInterceptors(ContainerBuilder builder);
	}
}
