namespace Thinktecture.Relay.Server
{
	public enum AcknowledgmentMode
	{
		/// <summary>
		/// Requests in the message queue will be automatically acknowledged
		/// by the on-premise before contacting the local target
		/// </summary>
		Default,

		/// <summary>
		/// Requests in the message queue will be automatically acknowledged
		/// by the RelayServer before sending to the on-premise
		/// </summary>
		Auto,

		/// <summary>
		/// Requests in the message queue need to be acknowledged manually
		/// </summary>
		Manual,
	}
}
