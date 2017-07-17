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
		private readonly ILogger _logger;
		private readonly string _path;


		private readonly CancellationTokenSource _cancellationTokenSource;

		public FilePostDataTemporaryStore(ILogger logger, IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_path = configuration?.TemporaryRequestStoragePath ?? throw new ArgumentNullException($"{nameof(configuration)}.{nameof(configuration.TemporaryRequestStoragePath)}");

			if (!Directory.Exists(_path))
				throw new ConfigurationErrorsException($"{nameof(FilePostDataTemporaryStore)}: The configured directory does not exist: '{_path}'");

			_cancellationTokenSource = new CancellationTokenSource();
			StartCleanUpTask();
		}

		private void StartCleanUpTask()
		{
			Task.Factory.StartNew(() =>
			{
				var cancellationToken = _cancellationTokenSource.Token;

				while (!cancellationToken.IsCancellationRequested)
				{
					if (!cancellationToken.WaitHandle.WaitOne(30 * 1000))
					{
						CleanUp();
					}
				}
			}, _cancellationTokenSource.Token);
		}

		private void CleanUp()
		{
			var timeOut = DateTime.UtcNow.AddSeconds(-10);

			try
			{
				foreach (var fileName in Directory.GetFiles(_path))
				{
					if (File.GetCreationTimeUtc(fileName) < timeOut)
					{
						File.Delete(fileName);
					}
				}
			}
			catch(Exception) { } // silently catch, multiple services could try to delete files at the same time
		}

		public void Save(string requestId, byte[] data)
		{
			_logger.Debug("Storing body for request id {0}", requestId);

			File.WriteAllBytes(GetFileName(requestId), data);
		}

		public byte[] Load(string requestId)
		{
			_logger.Debug("Loading body for request id {0}", requestId);

			var filename = GetFileName(requestId);
			var data = File.ReadAllBytes(filename);
			File.Delete(filename);

			return data;
		}

		private string GetFileName(string requestId)
		{
			return Path.Combine(_path, requestId + ".req");
		}

		public void Close()
		{
			_cancellationTokenSource.Cancel();
		}

		#region IDisposable

		~FilePostDataTemporaryStore()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			Close();
		}

		#endregion
	}
}
