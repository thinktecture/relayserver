namespace Thinktecture.Relay.Transport
{
	/// <summary>
	/// An implementation of a connector transport limit between connector and RelayServer.
	/// </summary>
	public interface IConnectorTransportLimit
	{
		/// <summary>
		/// The maximum size of binary data the transport is capable to serialize inline, or null if there is no limit.
		/// </summary>
		int? BinarySizeThreshold { get; }
	}
}
