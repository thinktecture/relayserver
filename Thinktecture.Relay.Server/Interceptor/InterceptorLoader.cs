using System;
using Autofac;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.DependencyInjection;

namespace Thinktecture.Relay.Server.Interceptor
{
	internal class InterceptorLoader : IInterceptorLoader
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly ICustomCodeAssemblyLoader _customCodeAssemblyLoader;

		public InterceptorLoader(ILogger logger, IConfiguration configuration, ICustomCodeAssemblyLoader customCodeAssemblyLoader)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_customCodeAssemblyLoader = customCodeAssemblyLoader ?? throw new ArgumentNullException(nameof(customCodeAssemblyLoader));
		}

		public void RegisterInterceptors(ContainerBuilder builder)
		{
			var assembly = _customCodeAssemblyLoader.Assembly;
			if (assembly == null)
				return;

			try
			{
				_logger?.Debug("Trying to register interceptors from custom code assembly. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);

				if (!RegisterInterceptors(builder, typeof(IOnPremiseRequestInterceptor), typeof(IOnPremiseResponseInterceptor)))
				{
					_logger?.Warning("No interceptors could be registered. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
					return;
				}

				_logger?.Information("Successfully registered interceptors from {CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "There was an error loading the interceptors. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
			}
		}

		private bool RegisterInterceptors(ContainerBuilder builder, params Type[] interfaceTypes)
		{
			var registered = false;

			foreach (var interfaceType in interfaceTypes)
			{
				_logger?.Verbose("Trying to load interceptor implementation. type={InterceptorInterface}", interfaceType.Name);

				var interceptorType = _customCodeAssemblyLoader.GetType(interfaceType);
				if (interceptorType != null)
				{
					_logger?.Verbose("Registering interceptor. type={InterceptorType}', interface={InterceptorInterface}", interceptorType.Name, interfaceType.Name);
					builder.RegisterType(interceptorType).As(interfaceType);
					registered = true;
				}
			}

			return registered;
		}
	}
}
