using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
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
			}, token).ConfigureAwait(false);
		}

		private void CleanUp(CancellationToken cancellationToken)
		{
			_logger?.Verbose("Cleaning up old stored files");

			var timeOut = DateTime.UtcNow.Add(_storagePeriod);

			try
			{
				foreach (var fileName in Directory.GetFiles(_path))
				{
					if (cancellationToken.IsCancellationRequested)
					{
						return;
					}

					try
					{
						if (File.GetCreationTimeUtc(fileName) < timeOut)
						{
							File.Delete(fileName);
						}
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Could not delete file. FileName = {FileName}", fileName);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during cleanup");
			}
		}

		public byte[] LoadRequest(string requestId)
		{
			var fileName = GetRequestFileName(requestId);
			_logger?.Verbose("Loading request body. request-id={RequestId}, file-name={FileName}", requestId, fileName);

			if (File.Exists(fileName))
			{
				var data = File.ReadAllBytes(fileName);

				try
				{
					File.Delete(fileName);
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Could not delete file. FileName = {FileName}", fileName);
				}

				return data;
			}

			return null;
		}

		public Stream CreateRequestStream(string requestId)
		{
			var fileName = GetRequestFileName(requestId);
			_logger?.Verbose("Creating stream for storing request body. request-id={RequestId}, file-name={FileName}", requestId, fileName);

			return File.Open(fileName, FileMode.Create);
		}

		public Stream GetRequestStream(string requestId)
		{
			var fileName = GetRequestFileName(requestId);
			_logger?.Verbose("Creating stream for stored request body. request-id={RequestId}, file-name={FileName}", requestId, fileName);

			if (File.Exists(fileName))
			{
				return File.Open(fileName, FileMode.Open);
			}

			return null;
		}

		public void SaveResponse(string requestId, byte[] data)
		{
			var fileName = GetResponseFileName(requestId);
			_logger?.Verbose("Storing response body. request id={RequestId}, file-name={FileName}", requestId, fileName);

			File.WriteAllBytes(fileName, data);
		}

		public Stream CreateResponseStream(string requestId)
		{
			var fileName = GetResponseFileName(requestId);
			_logger?.Verbose("Creating stream for storing response body. request-id={RequestId}, file-name={FileName}", requestId, fileName);

			return File.Open(fileName, FileMode.Create);
		}

		public Stream GetResponseStream(string requestId)
		{
			var fileName = GetResponseFileName(requestId);
			_logger?.Verbose("Creating stream for stored response body. request-id={RequestId}, file-name={FileName}", requestId, fileName);

			if (File.Exists(fileName))
			{
				return File.Open(fileName, FileMode.Open);
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

		public void Dispose()
		{
			Dispose(true);
			
		}

		~FilePostDataTemporaryStore()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();
			}

			GC.SuppressFinalize(this);
		}
	}
}
