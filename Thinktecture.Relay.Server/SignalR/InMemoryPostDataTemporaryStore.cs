using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.SignalR
{
	internal class InMemoryPostDataTemporaryStore : IPostDataTemporaryStore, IDisposable
	{
		private readonly ILogger _logger;
		private readonly TimeSpan _storagePeriod;

		private readonly ConcurrentDictionary<string, Entry> _data;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public InMemoryPostDataTemporaryStore(ILogger logger, IConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (configuration.TemporaryRequestStoragePeriod <= TimeSpan.Zero)
				throw new ArgumentException($"{nameof(InMemoryPostDataTemporaryStore)}: Storage period must be positive. Provided value: {configuration.TemporaryRequestStoragePeriod}", nameof(configuration));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_storagePeriod = configuration.TemporaryRequestStoragePeriod;
			_data = new ConcurrentDictionary<string, Entry>();
			_cancellationTokenSource = new CancellationTokenSource();

			StartCleanUpTask(_cancellationTokenSource.Token);
		}

		private void StartCleanUpTask(CancellationToken token)
		{
			Task.Run(() =>
			{
				while (!token.IsCancellationRequested)
				{
					if (!token.WaitHandle.WaitOne(1000))
						CleanUp();
				}
			}, token);
		}

		private void CleanUp()
		{
			foreach (var key in _data.Keys)
			{
				if (_data.TryGetValue(key, out var entry) && entry.IsTimedOut)
					_data.TryRemove(key, out entry);
			}
		}

		public void Save(string requestId, byte[] data)
		{
			_logger.Debug($"{nameof(InMemoryPostDataTemporaryStore)}: Storing body for request id {{0}}", requestId);

			_data[requestId] = new Entry(data, _storagePeriod);
		}

		public byte[] Load(string requestId)
		{
			_logger.Debug($"{nameof(InMemoryPostDataTemporaryStore)}: Loading body for request id {{0}}", requestId);

			return _data.TryRemove(requestId, out var entry) ? entry.Data : new byte[] { };
		}

		~InMemoryPostDataTemporaryStore()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}

		private class Entry
		{
			private readonly DateTime _timeoutDate;

			public byte[] Data { get; }

			public bool IsTimedOut => _timeoutDate < DateTime.UtcNow;

			public Entry(byte[] data, TimeSpan storagePeriod)
			{
				_timeoutDate = DateTime.UtcNow.Add(storagePeriod);
				Data = data;
			}
		}
	}
}
