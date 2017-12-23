using System;
using System.IO;
using Amazon.ElastiCacheCluster;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Enyim.Caching;

namespace Thinktecture.Relay.Server.PostDataTemporaryStore
{
	internal class MemcachedPostDataTemporaryStore : IPostDataTemporaryStore, IDisposable
	{
		private static readonly byte[] _emptyByteArray = new byte[0];

		private readonly ILogger _logger;
		private readonly TimeSpan _storagePeriod;
		private readonly MemcachedClient _memcachedClient;

		public MemcachedPostDataTemporaryStore(ILogger logger, IConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));
			if (String.IsNullOrWhiteSpace(configuration.TemporaryRequestStorageMemcachedNodeEndPoint) && String.IsNullOrWhiteSpace(configuration.TemporaryRequestStorageMemcachedConfigEndPoint))
				throw new ArgumentException($"{nameof(MemcachedPostDataTemporaryStore)}: Either Memcached Node or Config endpoint must be provided.", nameof(configuration));
			if (configuration.TemporaryRequestStoragePeriod <= TimeSpan.Zero)
				throw new ArgumentException($"{nameof(MemcachedPostDataTemporaryStore)}: Storage period must be positive. Provided value: {configuration.TemporaryRequestStoragePeriod}", nameof(configuration));

			_logger = logger;
			_storagePeriod = configuration.TemporaryRequestStoragePeriod;

			IMemcachedClientConfiguration memcachedConfig;
			if (!String.IsNullOrWhiteSpace(configuration.TemporaryRequestStorageMemcachedConfigEndPoint))
			{
				var memcachedConfigEndPoint = ParseEndPoint(configuration.TemporaryRequestStorageMemcachedConfigEndPoint, "config");
				memcachedConfig = new ElastiCacheClusterConfig(memcachedConfigEndPoint.Item1, memcachedConfigEndPoint.Item2);
			}
			else
			{
				memcachedConfig = new MemcachedClientConfiguration();
				var memcachedNodeEndPoint = ParseEndPoint(configuration.TemporaryRequestStorageMemcachedNodeEndPoint, "node");
				((MemcachedClientConfiguration) memcachedConfig).AddServer(memcachedNodeEndPoint.Item1, memcachedNodeEndPoint.Item2);
			}
			_memcachedClient = new MemcachedClient(memcachedConfig);
		}

		private Tuple<String, Int32> ParseEndPoint(String endpoint, String kind)
		{
			var port = 11211;
			var parts = endpoint.Split(new[] { ':' }, StringSplitOptions.None);
			if (parts.Length == 0 || parts.Length > 2)
				throw new ArgumentException($"{nameof(MemcachedPostDataTemporaryStore)}: Invalid endpoint for Memcached {kind}");
			if (parts.Length == 2)
				Int32.TryParse(parts[1], out port);
			return new Tuple<String, Int32>(parts[0], port);
		}

		public Byte[] LoadRequest(string requestId)
		{
			_logger?.Verbose("Loading request body. request-id={RequestId}", requestId);

			return _memcachedClient.Get(requestId) as Byte[] ?? _emptyByteArray;
		}

		public Stream CreateRequestStream(string requestId)
		{
			_logger?.Verbose("Creating stream for storing request body. request-id={RequestId}", requestId);

			var ms = new NotifyingMemoryStream();
			ms.Disposing += (s, e) => _memcachedClient.Store(StoreMode.Set, requestId, (s as NotifyingMemoryStream)?.ToArray(), _storagePeriod);
			return ms;
		}

		public Stream GetRequestStream(string requestId)
		{
			_logger?.Verbose("Creating stream for stored request body. request-id={RequestId}", requestId);

			return new MemoryStream(_memcachedClient.Get(requestId) as Byte[]);
		}

		public void SaveResponse(string requestId, byte[] data)
		{
			_logger?.Verbose("Storing response body. request id={RequestId}", requestId);

			_memcachedClient.Store(StoreMode.Set, requestId, data, _storagePeriod);
		}

		public Stream CreateResponseStream(string requestId)
		{
			_logger?.Verbose("Creating stream for storing response body. request-id={RequestId}", requestId);

			var ms = new NotifyingMemoryStream();
			ms.Disposing += (s, e) => _memcachedClient.Store(StoreMode.Set, requestId, (s as NotifyingMemoryStream)?.ToArray(), _storagePeriod);
			return ms;
		}

		public Stream GetResponseStream(string requestId)
		{
			_logger?.Verbose("Creating stream for stored response body. request-id={RequestId}", requestId);

			return new MemoryStream(_memcachedClient.Get(requestId) as Byte[]);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_memcachedClient.Dispose();
			}
		}
	}
}
