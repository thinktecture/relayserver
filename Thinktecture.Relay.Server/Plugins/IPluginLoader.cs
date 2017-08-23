using Autofac;

namespace Thinktecture.Relay.Server.Plugins
{
	internal interface IPluginLoader
	{
		void LoadPlugins(ContainerBuilder builder);
	}
}
