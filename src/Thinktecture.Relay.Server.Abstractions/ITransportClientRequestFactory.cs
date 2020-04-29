using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITransportClientRequest"/>.
	/// </summary>
	public interface ITransportClientRequestFactory<out TRequest>
		where TRequest : ITransportClientRequest
	{
		/// <summary>
		/// Create an instance of a class implementing <see cref="ITransportClientRequest"/> from <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The HTTP request data.</param>
		/// <returns>An instance implementing <see cref="ITransportClientRequest"/>.</returns>
		TRequest Create(object source); // TODO where does the data come from?
	}
}
