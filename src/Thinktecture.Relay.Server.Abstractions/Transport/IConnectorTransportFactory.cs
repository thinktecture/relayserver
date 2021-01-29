using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="IConnectorTransport{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of request.</typeparam>
	public interface IConnectorTransportFactory<in T>
		where T : IClientRequest
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="IConnectorTransport{T}"/>.
		/// </summary>
		/// <param name="connectionId">The unique connection id.</param>
		/// <returns>An <see cref="IConnectorTransport{T}"/>.</returns>
		IConnectorTransport<T> Create(string connectionId);
	}
}
