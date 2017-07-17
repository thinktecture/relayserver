using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Thinktecture.Relay.Server.SignalR
{
    internal class PostDataTemporaryStore : IPostDataTemporaryStore, IDisposable
    {
        private readonly ILogger _logger;

        private class Entry
        {
            private readonly DateTime _timeoutDate;

            public byte[] Data { get; private set; }

            public bool IsTimedOut
            {
                get { return _timeoutDate < DateTime.UtcNow; }
            }

            public Entry(byte[] data)
            {
                _timeoutDate = DateTime.UtcNow.AddSeconds(10);
                Data = data;
            }
        }

        private readonly ConcurrentDictionary<string, Entry> _data;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public PostDataTemporaryStore(ILogger logger)
        {
            _logger = logger;
            _data = new ConcurrentDictionary<string, Entry>();
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
                    if (!cancellationToken.WaitHandle.WaitOne(1000))
                    {
                        CleanUp();
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        private void CleanUp()
        {
            foreach (var key in _data.Keys)
            {
                Entry entry;
                if (_data.TryGetValue(key, out entry) && entry.IsTimedOut)
                {
                    _data.TryRemove(key, out entry);
                }
            }
        }

        public void Save(string requestId, byte[] data)
        {
            _logger.Debug("Storing body for request id {0}", requestId);

            _data[requestId] = new Entry(data);
        }

        public byte[] Load(string requestId)
        {
            _logger.Debug("Loading body for request id {0}", requestId);

            Entry entry;
            return _data.TryRemove(requestId, out entry) ? entry.Data : new byte[] { };
        }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
        }

        #region IDisposable

        ~PostDataTemporaryStore()
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
