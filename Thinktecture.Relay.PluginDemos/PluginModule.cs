using Autofac;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.PluginDemos
{
	/*
	/// <inheritdoc />
	/// <summary>
	/// A relay server plugin assembly needs to provide a single AutoFac Module, that will register all
	/// plugin types that are implemented and should be used.
	/// </summary>
	public class PluginModule : Module
	{
		/// <inheritdoc />
		/// <summary>
		/// Override the Load method of the AutoFac module to register the plugin types.
		/// </summary>
		/// <param name="builder"></param>
		protected override void Load(ContainerBuilder builder)
		{
			// Each plugin that should be added needs to be registered with the container builder as its Interface type
			builder.RegisterType<RequestHeaderManipulatorDemoPlugin>().As<IRequestHeaderManipulator>();
			builder.RegisterType<RequestMethodManipulatorDemoPlugin>().As<IRequestMethodManipulator>();
			builder.RegisterType<ResponseBodyManipulatorDemoPlugin>().As<IResponseBodyManipulator>();

			base.Load(builder);
		}
	}
	*/
}
