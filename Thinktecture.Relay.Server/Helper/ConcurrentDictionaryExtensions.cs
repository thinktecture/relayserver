using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Helper
{
	public static class ConcurrentDictionaryExtensions
	{
		public static async Task<TResult> GetOrAddAsync<TKey, TResult>(
			this ConcurrentDictionary<TKey, TResult> dictionary,
			SemaphoreSlim lockObject,
			TKey key,
			Func<TKey, Task<TResult>> asyncValueFactory
			)
		{
			if (dictionary.TryGetValue(key, out var result))
			{
				return result;
			}

			await lockObject.WaitAsync();
			try
			{
				if (dictionary.TryGetValue(key, out result))
				{
					return result;
				}

				return dictionary[key] = await asyncValueFactory(key);
			}
			finally
			{
				lockObject.Release();
			}
		}
	}
}
