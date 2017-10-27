using Autofac;
using Thinktecture.Relay.Server.Interceptor;

namespace Thinktecture.Relay.InterceptorDemos
{
	/// <inheritdoc />
	/// <summary>
	/// A relay server Interceptor assembly needs to provide a single AutoFac Module, that will register all
	/// interceptor types that are implemented and should be used.
	/// </summary>
	public class InterceptorModule : Module
	{
		/// <inheritdoc />
		/// <summary>
		/// Override the Load method of the AutoFac module to register the interceptor types.
		/// </summary>
		/// <param name="builder"></param>
		protected override void Load(ContainerBuilder builder)
		{
			// Each interceptor that should be added needs to be registered with the container builder as its Interface type
			builder.RegisterType<DemoRequestInterceptor>().As<IOnPremiseRequestInterceptor>();
			builder.RegisterType<DemoResponseInterceptor>().As<IOnPremiseResponseInterceptor>();

			base.Load(builder);
		}
	}
}
