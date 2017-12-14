namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	/// <summary>
	/// This is the pendant for the server enum AcknowledgementMode and should be kept separate
	/// </summary>
	public enum AcknowledgmentMode
	{
		/// <summary>
		/// Requests in the message queue will be automatically acknowledged
		/// by the on-premise before contacting the local target
		/// </summary>
		Default,

		/// <summary>
		/// Requests in the message queue will be automatically acknowledged
		/// by the relay server before sending to the on-premise
		/// </summary>
		Auto,

		/// <summary>
		/// Requests in the message queue need to be acknowledged manually
		/// </summary>
		Manual,
	}
}
