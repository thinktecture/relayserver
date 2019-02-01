using System.IO;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// This store is responsible for holding larger request and response bodies.
	/// </summary>
	public interface IPostDataTemporaryStore
	{
		/// <summary>
		/// Creates a stream, into which the request body will be written.
		/// </summary>
		/// <param name="requestId">The id of the request, used as name for the stream.</param>
		/// <returns>A new <see cref="Stream"/> that will be used to save the request data to.</returns>
		Stream CreateRequestStream(string requestId);

		/// <summary>
		/// Retrieves a stream from the store, to read the request body from.
		/// </summary>
		/// <param name="requestId">The id of the request, used as name of the stream.</param>
		/// <returns>A new <see cref="Stream"/> from which the request data will be read.</returns>
		Stream GetRequestStream(string requestId);

		/// <summary>
		/// Creates a new stream, into which the response body will be written.
		/// </summary>
		/// <param name="requestId">The id of the request, used as name for the stream.</param>
		/// <returns>A new <see cref="Stream"/> that will be used to save the response data to.</returns>
		Stream CreateResponseStream(string requestId);

		/// <summary>
		/// Retrieves a stream from the store, to read the response body from.
		/// </summary>
		/// <param name="requestId">The id of the request, used as name of the stream.</param>
		/// <returns>A new <see cref="Stream"/> from which the response data will be read.</returns>
		Stream GetResponseStream(string requestId);

		/// <summary>
		/// Renames a response stream that was created with a temporary id to the real response name.
		/// <remarks>This will only be used for legacy On-Premise Connectors (version &lt; 2.0)</remarks>
		/// </summary>
		/// <param name="temporaryId">The old request id</param>
		/// <param name="requestId">The new request id</param>
		/// <returns>The size of the stream contents; this must be accurate</returns>
		long RenameResponseStream(string temporaryId, string requestId);
	}
}
