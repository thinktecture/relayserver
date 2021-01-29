using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a response context containing the target response and an optional disposable.
	/// </summary>
	/// <typeparam name="T">The type of response.</typeparam>
	public interface IResponseContext<out T>
		where T : ITargetResponse
	{
		/// <summary>
		/// The target response.
		/// </summary>
		T Response { get; }

		/// <summary>
		/// An <see cref="IAsyncDisposable"/> which should be disposed when the response body content is no longer needed.
		/// </summary>
		IAsyncDisposable? Disposable { get; }
	}
}
