using System;
using Autofac;
using Autofac.Integration.WebApi;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.DependencyInjection;

namespace Thinktecture.Relay.Server.Controller
{
	internal class ControllerLoader : IControllerLoader
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly ICustomCodeAssemblyLoader _customCodeAssemblyLoader;

		public ControllerLoader(ILogger logger, IConfiguration configuration, ICustomCodeAssemblyLoader customCodeAssemblyLoader)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_customCodeAssemblyLoader = customCodeAssemblyLoader ?? throw new ArgumentNullException(nameof(customCodeAssemblyLoader));
		}

		public void RegisterControllers(ContainerBuilder builder)
		{
			var assembly = _customCodeAssemblyLoader.Assembly;
			if (assembly == null)
				return;

			_logger?.Debug("Trying to register controllers from custom code assembly. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);

			builder.RegisterApiControllers(assembly);
		}
	}
}
