using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a response context containing the response and an optional disposable.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IResponseContext<TResponse>
		where TResponse : class, ITargetResponse
	{
		/// <summary>
		/// The target response.
		/// </summary>
		TResponse Response { get; set; }

		/// <summary>
		/// An <see cref="IAsyncDisposable"/> which should be disposed when the response body content is no longer needed.
		/// </summary>
		IAsyncDisposable Disposable { get; set; }

		/// <summary>
		/// Deconstruction.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <param name="disposable">An <see cref="IAsyncDisposable"/>.</param>
		void Deconstruct(out TResponse response, out IAsyncDisposable disposable);
	}
}
