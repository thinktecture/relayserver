using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class FilePostDataTemporaryStore : IPostDataTemporaryStore, IDisposable
	{
		private static readonly TimeSpan _cleanupInterval = TimeSpan.FromSeconds(30);

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

			_logger = logger;
			_storagePeriod = configuration.TemporaryRequestStoragePeriod;
			_path = configuration.TemporaryRequestStoragePath;
			_cancellationTokenSource = new CancellationTokenSource();

			StartCleanUpTask(_cancellationTokenSource.Token);
		}

		private void StartCleanUpTask(CancellationToken token)
		{
			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
					CleanUp(token);
					await Task.Delay(_cleanupInterval, token).ConfigureAwait(false);
				}
			}, token);
		}

		private void CleanUp(CancellationToken cancellationToken)
		{
			_logger?.Trace($"{nameof(FilePostDataTemporaryStore)}: Cleaning up old stored files");

			var timeOut = DateTime.UtcNow.Add(_storagePeriod);

			try
			{
				foreach (var fileName in Directory.GetFiles(_path))
				{
					if (cancellationToken.IsCancellationRequested)
						return;

					try
					{
						if (File.GetCreationTimeUtc(fileName) < timeOut)
							File.Delete(fileName);
					}
					catch (Exception ex)
					{
						_logger?.Trace(ex, $"{nameof(FilePostDataTemporaryStore)}: Could not delete temp file {{0}}", fileName);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, $"{nameof(FilePostDataTemporaryStore)}: Error during cleanup");
			}
	    }

		public void SaveRequest(string requestId, byte[] data)
		{
			var fileName = GetRequestFileName(requestId);
			_logger?.Debug($"{nameof(FilePostDataTemporaryStore)}: Storing request body for request id {{0}} in {{1}}", requestId, fileName);

			File.WriteAllBytes(fileName, data);
		}

		public void SaveResponse(string requestId, byte[] data)
		{
			var fileName = GetResponseFileName(requestId);
			_logger?.Debug($"{nameof(FilePostDataTemporaryStore)}: Storing response body for request id {{0}} in {{1}}", requestId, fileName);

			File.WriteAllBytes(fileName, data);
		}

		public byte[] LoadRequest(string requestId)
		{
			var fileName = GetRequestFileName(requestId);
			_logger?.Debug($"{nameof(FilePostDataTemporaryStore)}: Loading request body for request id {{0}} from {{1}}", requestId, fileName);

			if (File.Exists(fileName))
			{
			    var data = File.ReadAllBytes(fileName);

			    try
			    {
			    	File.Delete(fileName);
			    }
			    catch (Exception ex)
		        {
			    	_logger?.Trace(ex, $"{nameof(FilePostDataTemporaryStore)}: Could not delete temp file {{0}}", fileName);
			    }

			    return data;
		    }

			return null;
		}

		public byte[] LoadResponse(string requestId)
		{
			var fileName = GetResponseFileName(requestId);
			_logger?.Debug($"{nameof(FilePostDataTemporaryStore)}: Loading response body for request id {{0}} from {{1}}", requestId, fileName);

			if (File.Exists(fileName))
			{
			    var data = File.ReadAllBytes(fileName);

			    try
			    {
				    File.Delete(fileName);
			    }
			    catch (Exception ex)
			    {
				    _logger?.Trace(ex, $"{nameof(FilePostDataTemporaryStore)}: Could not delete temp file {{0}}", fileName);
			    }

			    return data;
			}

			return null;
		}

		private string GetRequestFileName(string requestId)
		{
			return GetFileName(requestId, ".req");
		}

		private string GetResponseFileName(string requestId)
		{
			return GetFileName(requestId, ".res");
		}

		private string GetFileName(string requestId, string extension)
		{
			return Path.Combine(_path, requestId + extension);
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
