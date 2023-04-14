using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;

// ReSharper disable once CheckNamespace; (extension methods on OrderedQueryables namespace)
namespace System.Linq;

/// <summary>
/// Provides extension methods for the <see cref="IOrderedQueryable"/> type.
/// </summary>
public static class OrderedQueryableExtensions
{
	/// <summary>
	/// Creates a paged result from a query in an asynchronous fashion.
	/// </summary>
	/// <param name="source">The <see cref="IOrderedQueryable{T}"/> to query for a page.</param>
	/// <param name="skip">The amount of items to skip.</param>
	/// <param name="take">The amount of items to take.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is
	/// <see cref="P:System.Threading.CancellationToken.None"/>.
	/// </param>
	/// <typeparam name="T">The type of the <see cref="IOrderedQueryable"/>.</typeparam>
	/// <returns>A paged result containing the corresponding items.</returns>
	public static async Task<Page<T>> ToPagedResultAsync<T>(this IOrderedQueryable<T> source, int skip, int take,
		CancellationToken cancellationToken = default)
		=> new Page<T>()
			{
				TotalCount = await source.CountAsync(cancellationToken),
				Results = await source.Skip(skip).Take(take).ToArrayAsync(cancellationToken),
				Offset = skip,
				PageSize = take,
			};
}
