using System;
using Autofac;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.DependencyInjection;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class DataStoreLoader : IDataStoreLoader
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly ICustomCodeAssemblyLoader _customCodeAssemblyLoader;

		public DataStoreLoader(ILogger logger, IConfiguration configuration, ICustomCodeAssemblyLoader customCodeAssemblyLoader)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_customCodeAssemblyLoader = customCodeAssemblyLoader ?? throw new ArgumentNullException(nameof(customCodeAssemblyLoader));
		}

		public void RegisterDataStore(ContainerBuilder builder)
		{
			var assembly = _customCodeAssemblyLoader.Assembly;
			if (assembly == null)
				return;

			try
			{
				_logger?.Debug("Trying to register data store from custom code assembly. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);

				var dataStoreType = _customCodeAssemblyLoader.GetType(typeof(IPostDataTemporaryStore));
				if (dataStoreType != null)
				{
					_logger?.Verbose("Registering data store. type={DataStoreType}', interface={DataStoreInterface}", dataStoreType.Name, typeof(IPostDataTemporaryStore).Name);
					builder.RegisterType(dataStoreType).As(typeof(IPostDataTemporaryStore)).SingleInstance();
					_logger?.Information("Successfully registered data store from {CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
					return;
				}

				_logger?.Information("No data stores found to register in {CustomCodeAssemblyPath}.", _configuration.CustomCodeAssemblyPath);

			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "There was an error loading a data store. assembly-path={CustomCodeAssemblyPath}", _configuration.CustomCodeAssemblyPath);
			}

			RegisterInternalDataStore(builder);
		}

		private void RegisterInternalDataStore(ContainerBuilder builder)
		{
			if (!String.IsNullOrWhiteSpace(_configuration.TemporaryRequestStoragePath))
			{
				RegisterInternalDataStoreType<FilePostDataTemporaryStore>(builder);
			}
			else
			{
				RegisterInternalDataStoreType<InMemoryPostDataTemporaryStore>(builder);
			}
		}

		private void RegisterInternalDataStoreType<T>(ContainerBuilder builder)
		{
			builder.RegisterType<T>().As<IPostDataTemporaryStore>().SingleInstance();
			_logger?.Verbose("Registered internal data store type={DataStoreType}", typeof(T));
		}
	}
}
