using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptorLoader : IInterceptorLoader
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;

		public InterceptorLoader(ILogger logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public void LoadInterceptors(ContainerBuilder builder)
		{
			var assemblyPath = GetAssemblyPath();

			if (String.IsNullOrWhiteSpace(assemblyPath))
				return;

			try
			{
				_logger?.Debug("Trying to load interceptors from file '{interceptor-assembly}'", assemblyPath);

				var interceptorAssembly = Assembly.LoadFrom(assemblyPath);

				if (!RegisterInterceptorModule(interceptorAssembly, builder))
				{
					_logger?.Information("Interceptor assembly does not provide a DI module. Trying to load individual interceptor types.");

					if (!RegisterIndividualInterceptors(interceptorAssembly, builder))
					{
						_logger?.Warning("No interceptors could be registered from interceptor assembly.");
						return;
					}
				}

				_logger?.Information("Successfully loaded interceptor assembly");
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "There was an error loading the interceptor assembly. AssemblyPath = '{interceptor-assembly-path}'", assemblyPath);
			}
		}

		private string GetAssemblyPath()
		{
			var path = _configuration.InterceptorAssembly;

			if (String.IsNullOrWhiteSpace(path))
				return null;

			if (!File.Exists(path))
			{
				_logger?.Warning("An interceptor assembly has been configured, but it is not available at the configured path '{interceptor-assembly-path}'", path);
				return null;
			}

			return path;
		}

		private bool RegisterInterceptorModule(Assembly interceptorAssembly, ContainerBuilder builder)
		{
			var interceptorAutofacModule = LoadInterceptorModule(interceptorAssembly);
			if (interceptorAutofacModule == null)
				return false;

			var module = (IModule)Activator.CreateInstance(interceptorAutofacModule);
			builder.RegisterModule(module);

			return true;
		}

		private Type LoadInterceptorModule(Assembly interceptorAssembly)
		{
			// check for Module in assembly
			var autofacModules = interceptorAssembly.GetTypes().Where(t => t.IsAssignableTo<IModule>()).ToArray();

			if (autofacModules.Length == 0)
			{
				return null;
			}

			if (autofacModules.Length > 1)
			{
				_logger?.Warning("The interceptor assembly needs to provide at maximum of one (1) Autofac Module, but {interceptor-module-count} were found", autofacModules.Length);
				return null;
			}

			return autofacModules.Single();
		}

		private bool RegisterIndividualInterceptors(Assembly interceptorAssembly, ContainerBuilder builder)
		{
			var interceptorInterfaceTypes = new[] { typeof(IOnPremiseRequestInterceptor), typeof(IOnPremiseResponseInterceptor) };
			var registered = false;

			foreach (var interceptorInterfaceType in interceptorInterfaceTypes)
			{
				_logger?.Verbose("Trying to load interceptor. type={interceptor-interface}", interceptorInterfaceType.Name);

				var interceptorType = FindInterceptorType(interceptorInterfaceType, interceptorAssembly);
				if (interceptorType != null)
				{
					RegisterInterceptorType(interceptorType, interceptorInterfaceType, builder);
					registered = true;
				}
			}

			return registered;
		}

		private Type FindInterceptorType(Type interceptorInterfaceType, Assembly interceptorAssembly)
		{
			var foundInterceptors = interceptorAssembly.GetTypes()
				.Where(interceptorInterfaceType.IsAssignableFrom)
				.ToArray();

			if (foundInterceptors.Length == 0)
			{
				_logger?.Debug("Did not find a interceptor of type {interceptor-interface} in interceptor assembly", interceptorInterfaceType.Name);
				return null;
			}

			if (foundInterceptors.Length > 1)
			{
				_logger?.Warning("One kind of interceptor can only be registered once, but interceptor assembly provides {interceptor-amount} types that implement the {interceptor-interface} interceptor", foundInterceptors.Length, interceptorInterfaceType.Name);
				return null;
			}

			return foundInterceptors.Single();
		}

		private void RegisterInterceptorType(Type interceptorType, Type interceptorInterfaceType, ContainerBuilder builder)
		{
			_logger?.Verbose("Registering interceptor. type={interceptor-type}', interface={interceptor-interface}", interceptorType.Name, interceptorInterfaceType.Name);
			builder.RegisterType(interceptorType).As(interceptorInterfaceType);
		}
	}
}
