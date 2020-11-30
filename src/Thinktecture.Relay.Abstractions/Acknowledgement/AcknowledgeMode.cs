using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Acknowledgement
{
	/// <summary>
	/// Used to set the acknowledge mode on a <see cref="IClientRequest"/>.
	/// </summary>
	public enum AcknowledgeMode
	{
		/// <summary>
		/// Disables acknowledging of <see cref="ClientRequest"/>.
		/// </summary>
		/// <remarks>This is the default value.</remarks>
		Disabled,

		/// <summary>
		/// Acknowledges when the connector has received the <see cref="IClientRequest"/> and also downloaded an optional body.
		/// </summary>
		ConnectorReceived,

		/// <summary>
		/// Acknowledges when the connector has finished the <see cref="ClientRequest"/> and is about to return the <see cref="ITargetResponse"/> to the server (after uploading an optional body).
		/// </summary>
		ConnectorFinished,

		/// <summary>
		/// Acknowledgement will be done by third-party code.
		/// </summary>
		Manual
	}
}
