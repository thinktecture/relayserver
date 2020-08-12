using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class InMemoryBodyStore : IBodyStore
	{
		private readonly ConcurrentDictionary<Guid, byte[]> _requestStore = new ConcurrentDictionary<Guid, byte[]>();
		private readonly ConcurrentDictionary<Guid, byte[]> _responseStore = new ConcurrentDictionary<Guid, byte[]>();

		/// <inheritdoc />
		public async Task<long> StoreRequestBodyAsync(Guid requestId, Stream bodyStream, CancellationToken cancellationToken = default)
			=> await StoreDataAsync(requestId, bodyStream, _requestStore, cancellationToken);

		/// <inheritdoc />
		public async Task<long> StoreResponseBodyAsync(Guid requestId, Stream bodyStream, CancellationToken cancellationToken = default)
			=> await StoreDataAsync(requestId, bodyStream, _responseStore, cancellationToken);

		/// <inheritdoc />
		public async Task<Stream> OpenRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
			=> await GetStreamAsync(requestId, _requestStore);

		/// <inheritdoc />
		public async Task<Stream> OpenResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
			=> await GetStreamAsync(requestId, _responseStore);

		/// <inheritdoc />
		public Task RemoveRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			_requestStore.Remove(requestId, out _);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task RemoveResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		{
			_responseStore.Remove(requestId, out _);
			return Task.CompletedTask;
		}

		private async Task<long> StoreDataAsync(Guid id, Stream stream, ConcurrentDictionary<Guid, byte[]> store,
			CancellationToken cancellationToken)
		{
			await using var ms = new MemoryStream();

			if (stream.CanSeek)
			{
				stream.Position = 0;
			}

			await stream.CopyToAsync(ms, cancellationToken);

			store.TryAdd(id, ms.ToArray());
			return ms.Length;
		}

		private Task<MemoryStream> GetStreamAsync(Guid id, ConcurrentDictionary<Guid, byte[]> store)
			=> Task.FromResult(new MemoryStream(store[id]));
	}
}
