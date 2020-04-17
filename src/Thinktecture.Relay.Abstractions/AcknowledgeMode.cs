namespace Thinktecture.Relay.Abstractions
{
	/// <summary>
	/// Used to set the acknowledge mode on a <see cref="RelayClientRequest"/>.
	/// </summary>
	public enum AcknowledgeMode
	{
		/// <summary>
		/// Acknowledges when the connector has received the <see cref="RelayClientRequest"/> and also downloaded an optional body.
		/// </summary>
		ConnectorReceived,

		/// <summary>
		/// Acknowledges when the connector has finished the <see cref="RelayClientRequest"/> and is about to return the
		/// <see cref="RelayTargetResponse"/> to the server (after uploading an optional body).
		/// </summary>
		ConnectorFinished,

		/// <summary>
		/// Acknowledgement will be done by third-party code.
		/// </summary>
		Manual,

		/// <summary>
		/// Disables acknowledging of <see cref="RelayClientRequest"/>.
		/// </summary>
		Disabled
	}
}
