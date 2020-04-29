using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Connector.Abstractions
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITransportTargetResponse"/>.
	/// </summary>
	public interface ITransportTargetResponseFactory<out TResponse>
		where TResponse : ITransportTargetResponse
	{
		/// <summary>
		/// Create an instance of a class implementing <see cref="ITransportTargetResponse"/> from <paramref name="source"/>.
		/// </summary>
		/// <param name="source">The HTTP response data.</param>
		/// <returns>An instance implementing <see cref="ITransportTargetResponse"/>.</returns>
		TResponse Create(object source); // TODO where does the data come from?
	}
}
