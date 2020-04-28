namespace Thinktecture.Relay.Abstractions
{
	/// <summary>
	/// The metadata of a target response to be transported.
	/// </summary>
	public interface ITransportTargetResponse : IRelayTargetResponse
	{
		/// <summary>
		/// An array of <see cref="byte"/>s containing the body returned by the requested target.
		/// </summary>
		/// <seealso cref="IRelayTargetResponse.IsBodyAvailable"/>
		/// <remarks>This will be <value>null</value> when the body is too big for inlining. Setting a byte array on this property
		/// resets <see cref="IRelayTargetResponse.BodyStream"/> to <value>null</value></remarks>
		byte[] Body { get; set; }
	}
}
