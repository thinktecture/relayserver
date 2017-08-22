using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class FilePostDataTemporaryStore : IPostDataTemporaryStore, IDisposable
	{
		private readonly TimeSpan _storagePeriod;
		private readonly ILogger _logger;
		private readonly string _path;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public FilePostDataTemporaryStore(ILogger logger, IConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (String.IsNullOrEmpty(configuration.TemporaryRequestStoragePath))
				throw new ConfigurationErrorsException($"The path {nameof(configuration.TemporaryRequestStoragePath)} cannot be null or empty.");
			if (!Directory.Exists(configuration.TemporaryRequestStoragePath))
				throw new ConfigurationErrorsException($"{nameof(FilePostDataTemporaryStore)}: The configured directory does not exist: '{_path}'");
			if (configuration.TemporaryRequestStoragePeriod <= TimeSpan.Zero)
				throw new ArgumentException($"{nameof(FilePostDataTemporaryStore)}: Storage period must be positive. Provided value: {configuration.TemporaryRequestStoragePeriod}", nameof(configuration));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_storagePeriod = configuration.TemporaryRequestStoragePeriod;
			_path = configuration.TemporaryRequestStoragePath;
			_cancellationTokenSource = new CancellationTokenSource();

			StartCleanUpTask(_cancellationTokenSource.Token);
		}

		private void StartCleanUpTask(CancellationToken token)
		{
			Task.Run(() =>
			{
				while (!token.IsCancellationRequested)
				{
					if (!token.WaitHandle.WaitOne(30 * 1000))
						CleanUp();
				}
			}, token);
		}

		private void CleanUp()
		{
			var timeOut = DateTime.UtcNow.Add(_storagePeriod);

			try
			{
				foreach (var fileName in Directory.GetFiles(_path))
				{
					try
					{
						if (File.GetCreationTimeUtc(fileName) < timeOut)
							File.Delete(fileName);
					}
					catch (Exception ex)
					{
						_logger.Trace(ex, $"{nameof(FilePostDataTemporaryStore)}: Could not delete temp file {{0}}", fileName);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(FilePostDataTemporaryStore)}: Error during cleanup");
			}
		}

		public void Save(string requestId, byte[] data)
		{
			_logger.Debug("Storing body for request id {0}", requestId);

			File.WriteAllBytes(GetFileName(requestId), data);
		}

		public byte[] Load(string requestId)
		{
			_logger.Debug("Loading body for request id {0}", requestId);

			var fileName = GetFileName(requestId);
			var data = File.ReadAllBytes(fileName);

			try
			{
				File.Delete(fileName);
			}
			catch (Exception ex)
			{
				_logger.Trace(ex, $"{nameof(FilePostDataTemporaryStore)}: Could not delete temp file {{0}}", fileName);
			}

			return data;
		}

		private string GetFileName(string requestId)
		{
			return Path.Combine(_path, requestId + ".req");
		}

		~FilePostDataTemporaryStore()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}
	}
}
