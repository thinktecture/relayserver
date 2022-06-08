using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

/// <inheritdoc/>
internal class InMemoryBodyStore : IBodyStore
{
	private readonly ConcurrentDictionary<Guid, byte[]> _requestStore = new ConcurrentDictionary<Guid, byte[]>();
	private readonly ConcurrentDictionary<Guid, byte[]> _responseStore = new ConcurrentDictionary<Guid, byte[]>();

	public InMemoryBodyStore(ILogger<InMemoryBodyStore> logger)
		=> logger.LogDebug(21100, "Using {StorageType} as body store", nameof(InMemoryBodyStore));

	/// <inheritdoc/>
	public async Task<long> StoreRequestBodyAsync(Guid requestId, Stream bodyStream,
		CancellationToken cancellationToken = default)
		=> await StoreDataAsync(requestId, bodyStream, _requestStore, cancellationToken);

	/// <inheritdoc/>
	public async Task<long> StoreResponseBodyAsync(Guid requestId, Stream bodyStream,
		CancellationToken cancellationToken = default)
		=> await StoreDataAsync(requestId, bodyStream, _responseStore, cancellationToken);

	/// <inheritdoc/>
	public async Task<Stream> OpenRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		=> await GetStreamAsync(requestId, _requestStore);

	/// <inheritdoc/>
	public async Task<Stream> OpenResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
		=> await GetStreamAsync(requestId, _responseStore);

	/// <inheritdoc/>
	public Task RemoveRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
	{
		_requestStore.Remove(requestId, out _);
		return Task.CompletedTask;
	}

	/// <inheritdoc/>
	public Task RemoveResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default)
	{
		_responseStore.Remove(requestId, out _);
		return Task.CompletedTask;
	}

	/// <inheritdoc/>
	public IAsyncDisposable GetRequestBodyRemoveDisposable(Guid requestId)
		=> new DisposeAction(() => RemoveRequestBodyAsync(requestId));

	/// <inheritdoc/>
	public IAsyncDisposable GetResponseBodyRemoveDisposable(Guid requestId)
		=> new DisposeAction(() => RemoveResponseBodyAsync(requestId));

	private async Task<long> StoreDataAsync(Guid id, Stream stream, ConcurrentDictionary<Guid, byte[]> store,
		CancellationToken cancellationToken)
	{
		await using var memoryStream = await stream.CopyToMemoryStreamAsync(cancellationToken);
		store.TryAdd(id, memoryStream.ToArray());
		return memoryStream.Length;
	}

	private Task<MemoryStream> GetStreamAsync(Guid id, ConcurrentDictionary<Guid, byte[]> store)
		=> Task.FromResult(new MemoryStream(store[id]));
}
