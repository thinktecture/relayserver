using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class InMemoryPostDataTemporaryStore : IPostDataTemporaryStore, IDisposable
	{
		private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

		private readonly ILogger _logger;
		private readonly TimeSpan _storagePeriod;

		private readonly ConcurrentDictionary<string, Entry> _requestData;
		private readonly ConcurrentDictionary<string, Entry> _responseData;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public InMemoryPostDataTemporaryStore(ILogger logger, IConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (configuration.TemporaryRequestStoragePeriod <= TimeSpan.Zero)
				throw new ArgumentException($"{nameof(InMemoryPostDataTemporaryStore)}: Storage period must be positive. Provided value: {configuration.TemporaryRequestStoragePeriod}", nameof(configuration));

			_logger = logger;
			_storagePeriod = configuration.TemporaryRequestStoragePeriod;
			_requestData = new ConcurrentDictionary<string, Entry>();
			_responseData = new ConcurrentDictionary<string, Entry>();
			_cancellationTokenSource = new CancellationTokenSource();

			StartCleanUpTask(_cancellationTokenSource.Token);
		}

		private void StartCleanUpTask(CancellationToken token)
		{
			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
					CleanUp();
					await Task.Delay(_cleanupInterval, token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
		}

		private void CleanUp()
		{
			_logger?.Verbose("Cleaning up old stored data");

			foreach (var kvp in _requestData)
			{
				if (kvp.Value.IsTimedOut)
				{
					_requestData.TryRemove(kvp.Key, out var value);
				}
			}

			foreach (var kvp in _responseData)
			{
				if (kvp.Value.IsTimedOut)
				{
					_requestData.TryRemove(kvp.Key, out var value);
				}
			}
		}

		public Stream CreateRequestStream(string requestId)
		{
			_logger?.Verbose("Creating write stream for storing request body. request-id={RequestId}", requestId);

			var ms = new NotifyingMemoryStream();
			ms.Disposing += (s, e) => _requestData[requestId] = new Entry(ms.ToArray(), _storagePeriod);
			return ms;
		}

		public Stream GetRequestStream(string requestId)
		{
			_logger?.Verbose("Creating read stream for stored request body. request-id={RequestId}", requestId);

			return _requestData.TryRemove(requestId, out var entry) ? new MemoryStream(entry.Data) : null;
		}

		public Stream CreateResponseStream(string requestId)
		{
			_logger?.Verbose("Creating write stream for storing response body. request-id={RequestId}", requestId);

			var ms = new NotifyingMemoryStream();
			ms.Disposing += (s, e) => _responseData[requestId] = new Entry(ms.ToArray(), _storagePeriod);
			return ms;
		}

		public Stream GetResponseStream(string requestId)
		{
			_logger?.Verbose("Creating read stream for stored response body. request-id={RequestId}", requestId);

			return _responseData.TryRemove(requestId, out var entry) ? new MemoryStream(entry.Data) : null;
		}

		public long RenameResponseStream(string temporaryId, string requestId)
		{
			_logger?.Verbose("Renaming stored response body. temporary-id={TemporaryId}, request-id={RequestId}", temporaryId, requestId);

			if (_responseData.TryRemove(temporaryId, out var entry))
			{
				_responseData[requestId] = entry;
				return entry.Data.LongLength;
			}

			return 0;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();
			}
		}

		private class Entry
		{
			private readonly DateTime _timeoutDate;

			public readonly byte[] Data;

			public bool IsTimedOut => _timeoutDate < DateTime.UtcNow;

			public Entry(byte[] data, TimeSpan storagePeriod)
			{
				_timeoutDate = DateTime.UtcNow.Add(storagePeriod);
				Data = data;
			}
		}
	}
}
