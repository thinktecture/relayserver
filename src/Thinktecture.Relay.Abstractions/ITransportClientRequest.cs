namespace Thinktecture.Relay.Abstractions
{
	/// <summary>
	/// The metadata of a client request to be transported.
	/// </summary>
	public interface ITransportClientRequest : IRelayClientRequest
	{
		/// <summary>
		/// An array of <see cref="byte"/>s containing the body provided by the requesting client.
		/// </summary>
		/// <seealso cref="IRelayClientRequest.IsBodyAvailable"/>
		/// <remarks>This will be <value>null</value> when the body is too big for inlining. Setting a byte array on this property
		/// resets <see cref="IRelayClientRequest.BodyStream"/> to <value>null</value>.</remarks>
		byte[] Body { get; set; }
	}
}
