using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Acknowledgement
{
	/// <summary>
	/// Used to set the acknowledge mode on a <see cref="ClientRequest"/>.
	/// </summary>
	public enum AcknowledgeMode
	{
		/// <summary>
		/// Acknowledges when the connector has received the <see cref="ClientRequest"/> and also downloaded an optional body.
		/// </summary>
		ConnectorReceived,

		/// <summary>
		/// Acknowledges when the connector has finished the <see cref="ClientRequest"/> and is about to return the
		/// <see cref="TargetResponse"/> to the server (after uploading an optional body).
		/// </summary>
		ConnectorFinished,

		/// <summary>
		/// Acknowledgement will be done by third-party code.
		/// </summary>
		Manual,

		/// <summary>
		/// Disables acknowledging of <see cref="ClientRequest"/>.
		/// </summary>
		Disabled
	}
}
