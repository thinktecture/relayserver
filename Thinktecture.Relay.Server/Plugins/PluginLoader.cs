using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NLog;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.Plugins
{
	internal class PluginLoader : IPluginLoader
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;

		public PluginLoader(ILogger logger, IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public void LoadPlugins(IContainer container)
		{
			var assemblyPath = GetAssemblyPath();
			if (string.IsNullOrWhiteSpace(assemblyPath))
				return;

			try
			{
				_logger.Trace($"{nameof(PluginLoader)}: Trying to load plugins from file '{{0}}'", assemblyPath);

				Assembly pluginAssembly = Assembly.LoadFrom(assemblyPath);
				
				if (!RegisterPluginModule(pluginAssembly, container))
				{
					_logger.Info($"{nameof(PluginLoader)}: Plugin assembly does not provide a DI module. Trying to load individual plugin types.");

					if (!RegisterIndividualPlugins(pluginAssembly, container))
					{
						_logger.Warn($"{nameof(PluginLoader)}: No plugins could be registered from plugin assembly.");
						return;
					}
				}

				_logger.Info($"{nameof(PluginLoader)}: Successfully loaded plugin assembly");
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(PluginLoader)}: There was an error loading the plugin assembly '{{0}}'", assemblyPath);
			}
		}

		private string GetAssemblyPath()
		{
			var path = _configuration.PluginAssembly;

			if (string.IsNullOrWhiteSpace(path))
				return null;
			
			if (!File.Exists(path))
			{
				_logger.Warn($"{nameof(PluginLoader)}: A plugin assembly has been configured, but it is not available. Configured path: '{{0}}'", path);
				return null;
			}

			return path;
		}

		private bool RegisterPluginModule(Assembly pluginAssembly, IContainer container)
		{
			Type pluginAutofacModule = LoadPluginModule(pluginAssembly);
			if (pluginAutofacModule == null)
				return false;

			RegisterModuleInContainer(pluginAutofacModule, container);

			return true;
		}

		private Type LoadPluginModule(Assembly pluginAssembly)
		{
			// check for Module in assembly
			var autofacModules = pluginAssembly.GetTypes().Where(t => t.IsAssignableTo<IModule>()).ToArray();

			if (autofacModules.Length == 0)
			{
				return null;
			}

			if (autofacModules.Length > 1)
			{
				_logger.Warn($"{nameof(PluginLoader)}: The plugin assembly needs to provide at maximum one Autofac Module, but {{0}} were found.", autofacModules.Length);
				return null;
			}

			return autofacModules.Single();
		}

		private void RegisterModuleInContainer(Type pluginAutofacModule, IContainer container)
		{
			var builder = new ContainerBuilder();

			IModule module = (IModule)Activator.CreateInstance(pluginAutofacModule);
			builder.RegisterModule(module);

			builder.Update(container);
		}

		private bool RegisterIndividualPlugins(Assembly pluginAssembly, IContainer container)
		{
			var pluginInterfaceTypes = DeterminePluginInterfaces();

			var builder = new ContainerBuilder();

			bool registered = false;
			foreach (var pluginInterfaceType in pluginInterfaceTypes)
			{
				_logger.Trace($"{nameof(PluginLoader)}: Trying to load plugins for type {{0}}", pluginInterfaceType.Name);

				var pluginType = FindPluginType(pluginInterfaceType, pluginAssembly);
				if (pluginType != null)
				{
					RegisterPluginType(pluginType, pluginInterfaceType, builder);
					registered = true;
				}
			}

			if (registered)
			{
				builder.Update(container);
			}

			return registered;
		}

		private Type[] DeterminePluginInterfaces()
		{
			// find all interfaces in the Plugin namespace
			var pluginType = typeof(IRequestMethodManipulator);
			return pluginType.Assembly.GetTypes()
				.Where(t => t.IsInterface && t.Namespace == pluginType.Namespace)
				.ToArray();
		}

		private Type FindPluginType(Type pluginInterfaceType, Assembly pluginAssembly)
		{
			var foundPlugins = pluginAssembly.GetTypes()
				.Where(pluginInterfaceType.IsAssignableFrom)
				.ToArray();

			if (foundPlugins.Length == 0)
			{
				_logger.Debug($"{nameof(PluginLoader)}: Did not find a plugin of type {{0}} in plugin assembly.", pluginInterfaceType.Name);
				return null;
			}

			if (foundPlugins.Length > 1)
			{
				_logger.Warn($"{nameof(PluginLoader)}: One kind of plugin can only be registered once, but plugin assembly provides {{0}} types that implement the {{1}} plugin.", foundPlugins.Length, pluginInterfaceType.Name);
				return null;
			}

			return foundPlugins.Single();
		}

		private void RegisterPluginType(Type pluginType, Type pluginInterfaceType, ContainerBuilder builder)
		{
			_logger.Debug($"{nameof(PluginLoader)}: Registering '{{0}}' as '{{1}}'", pluginType.Name, pluginInterfaceType.Name);
			builder.RegisterType(pluginType).As(pluginInterfaceType);
		}

	}
}
