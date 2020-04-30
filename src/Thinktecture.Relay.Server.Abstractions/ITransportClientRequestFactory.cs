using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITransportClientRequest"/>.
	/// </summary>
	public interface ITransportClientRequestFactory<out TRequest>
		where TRequest : ITransportClientRequest, new()
	{
		/// <summary>
		/// Create an instance of a class implementing <see cref="ITransportClientRequest"/> from <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
		/// <returns>An instance implementing <see cref="ITransportClientRequest"/>.</returns>
		TRequest Create(HttpContext context);
	}
}
