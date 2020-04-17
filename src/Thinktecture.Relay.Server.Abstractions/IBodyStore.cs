using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a store to persist the body of a request and response.
	/// </summary>
	public interface IBodyStore
	{
		/// <summary>
		/// Stores the request body stream for the request id.
		/// </summary>
		/// <param name="requestId">The id of the request.</param>
		/// <param name="bodyStream">The request stream to store.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task StoreRequestBodyAsync(Guid requestId, Stream bodyStream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Stores the response body stream for the request id.
		/// </summary>
		/// <param name="requestId">The id of the request.</param>
		/// <param name="bodyStream">The response stream to store.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task StoreResponseBodyAsync(Guid requestId, Stream bodyStream, CancellationToken cancellationToken = default);

		/// <summary>
		/// Opens the request body stream for the request id.
		/// </summary>
		/// <param name="requestId">The id of the request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the stream to the request body.</returns>
		Task<Stream> OpenRequestBodyAsync(Guid requestId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Opens the response body stream for the request id.
		/// </summary>
		/// <param name="requestId">The id of the request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the stream to the response body.</returns>
		Task<Stream> OpenResponseBodyAsync(Guid requestId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes the stored request body.
		/// </summary>
		/// <param name="requestId">The id of the request.</param>
		/// <param name="keepLatest"><value>true</value> to keep the latest version; <value>false</value> to remove all. The default value is <value>true</value></param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RemoveRequestBodyAsync(Guid requestId, bool keepLatest = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes the stored response body.
		/// </summary>
		/// <param name="requestId">The id of the request.</param>
		/// <param name="keepLatest"><value>true</value> to keep the latest version; <value>false</value> to remove all. The default value is <value>true</value></param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RemoveResponseBodyAsync(Guid requestId, bool keepLatest = false, CancellationToken cancellationToken = default);
	}
}
