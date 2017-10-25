using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NLog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Interceptors
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
				_logger?.Trace($"{nameof(InterceptorLoader)}: Trying to load interceptors from file '{{0}}'", assemblyPath);

				var interceptorAssembly = Assembly.LoadFrom(assemblyPath);

				if (!RegisterInterceptorModule(interceptorAssembly, builder))
				{
					_logger?.Info($"{nameof(InterceptorLoader)}: Interceptor assembly does not provide a DI module. Trying to load individual interceptor types.");

					if (!RegisterIndividualInterceptors(interceptorAssembly, builder))
					{
						_logger?.Warn($"{nameof(InterceptorLoader)}: No interceptors could be registered from interceptor assembly.");
						return;
					}
				}

				_logger?.Info($"{nameof(InterceptorLoader)}: Successfully loaded interceptor assembly");
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, $"{nameof(InterceptorLoader)}: There was an error loading the interceptor assembly '{{0}}'", assemblyPath);
			}
		}

		private string GetAssemblyPath()
		{
			var path = _configuration.InterceptorAssembly;

			if (String.IsNullOrWhiteSpace(path))
				return null;

			if (!File.Exists(path))
			{
				_logger?.Warn($"{nameof(InterceptorLoader)}: An interceptor assembly has been configured, but it is not available. Configured path: '{{0}}'", path);
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
				_logger?.Warn($"{nameof(InterceptorLoader)}: The interceptor assembly needs to provide at maximum of one (1) Autofac Module, but {{0}} were found.", autofacModules.Length);
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
				_logger?.Trace($"{nameof(InterceptorLoader)}: Trying to load interceptors for type {{0}}", interceptorInterfaceType.Name);

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
				_logger?.Debug($"{nameof(InterceptorLoader)}: Did not find a interceptor of type {{0}} in interceptor assembly.", interceptorInterfaceType.Name);
				return null;
			}

			if (foundInterceptors.Length > 1)
			{
				_logger?.Warn($"{nameof(InterceptorLoader)}: One kind of interceptor can only be registered once, but interceptor assembly provides {{0}} types that implement the {{1}} interceptor.", foundInterceptors.Length, interceptorInterfaceType.Name);
				return null;
			}

			return foundInterceptors.Single();
		}

		private void RegisterInterceptorType(Type interceptorType, Type interceptorInterfaceType, ContainerBuilder builder)
		{
			_logger?.Debug($"{nameof(InterceptorLoader)}: Registering '{{0}}' as '{{1}}'", interceptorType.Name, interceptorInterfaceType.Name);
			builder.RegisterType(interceptorType).As(interceptorInterfaceType);
		}
	}
}
