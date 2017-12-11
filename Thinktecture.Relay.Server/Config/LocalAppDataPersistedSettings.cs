using System;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace Thinktecture.Relay.Server.Config
{
	public class LocalAppDataPersistedSettings : IPersistedSettings
	{
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly string _settingsFileName;

		public Guid OriginId { get; private set; }

		public LocalAppDataPersistedSettings(ILogger logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			_settingsFileName = DetermineSettingsFileName();

			Load();

			if (OriginId == Guid.Empty)
			{
				OriginId = Guid.NewGuid();
				Store();
			}
		}

		/// <summary>
		/// Determines the file name to store the settings in.
		/// This uses the LocalAppData folder (user specific) and also uses the PORT of the application for the
		/// file name, to be able to run multiple RelayServers on the same host with different persisted settings.
		/// </summary>
		/// <returns>The file name to store the persisted settings in</returns>
		private string DetermineSettingsFileName()
		{
			var path = Environment.GetEnvironmentVariable("LocalAppData");
			if (String.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			}

			var fileName = Path.Combine(path, "Thinktecure", "RelayServer", $"settings_{_configuration.Port}.config.json");
		
			_logger?.Debug("Using settings file {SettingsFileName}", fileName);

			return fileName;
		}

		private void Load()
		{
			if (File.Exists(_settingsFileName))
			{
				var fileContents = File.ReadAllText(_settingsFileName);
				var configFromFile = JsonConvert.DeserializeObject<PersistedSettings>(fileContents);

				OriginId = configFromFile.OriginId;
				_logger?.Debug("Loaded setting with origin id {OriginId}", OriginId);
			}
			else
			{
				_logger?.Debug("No setting file available");
			}
		}

		private void Store()
		{
			var configData = JsonConvert.SerializeObject(this);
			var directory = Path.GetDirectoryName(_settingsFileName);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(_settingsFileName, configData);

			_logger?.Debug("Stored setting with origin id {OriginId}", OriginId);
		}

		// ReSharper disable once ClassNeverInstantiated.Local; Justification: It is instanciated by deserialization through Newtonsoft.Json
		private class PersistedSettings : IPersistedSettings
		{
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public Guid OriginId { get; set; }
		}
	}
}
