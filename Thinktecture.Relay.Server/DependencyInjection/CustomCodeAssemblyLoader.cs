using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	internal class CustomCodeAssemblyLoader : ICustomCodeAssemblyLoader
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;

		private Assembly _assembly;

		public CustomCodeAssemblyLoader(ILogger logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public Assembly Assembly => _assembly ?? (_assembly = LoadAssembly());

		private Assembly LoadAssembly()
		{
			if (_configuration.CustomCodeAssemblyPath == null)
				return null;

			try
			{
				_logger?.Debug("Trying to load custom code assembly. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
				var assembly = Assembly.LoadFrom(_configuration.CustomCodeAssemblyPath);

				_logger?.Information("Successfully loaded custom code assembly from {CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);

				return assembly;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "There was an error loading the custom code assembly. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
				_configuration.CustomCodeAssemblyPath = null;
				_assembly = null;
			}

			return null;
		}

		public void RegisterModule(ContainerBuilder builder)
		{
			var assembly = Assembly;
			if (assembly == null)
				return;

			try
			{
				var types = GetTypes(assembly, typeof(IModule));
				if (types.Length == 0)
				{
					_logger?.Information("Custom code assembly does not provide a DI module. Trying to load individual types later.");
					return;
				}

				if (types.Length > 1)
				{
					_logger?.Warning("The custom code assembly needs to provide a maximum of one (1) DI module, but more were found. module-count={ModuleCount}", types.Length);
					return;
				}

				builder.RegisterModule((IModule)Activator.CreateInstance(types.Single()));

				_logger?.Information("Successfully registered DI module from {CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);

				_configuration.CustomCodeAssemblyPath = null;
				_assembly = null;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "There was an error loading the DI module. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
			}
		}

		public Type GetType(Type type)
		{
			var types = GetTypes(type);
			if (types == null)
				return null;

			if (types.Length > 1)
			{
				_logger?.Warning("Only one kind of implementation can exist, but custom code assembly has multiple ones. implementation-amount={ImplementationAmount}, type={Type}", types.Length, type.Name);
				return null;
			}

			return types.Single();
		}

		public Type[] GetTypes(Type type)
		{
			var assembly = Assembly;
			if (assembly == null)
				return null;

			var types = GetTypes(assembly, type);
			if (types.Length == 0)
			{
				_logger?.Debug("Did not find any implementations in custom code assembly. type={Type}", type.Name);
			}

			return types;
		}

		private Type[] GetTypes(Assembly assembly, Type type)
		{
			return assembly.GetTypes().Where(type.IsAssignableFrom).ToArray();
		}
	}
}
