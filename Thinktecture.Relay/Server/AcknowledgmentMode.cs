namespace Thinktecture.Relay.Server
{
	public enum AcknowledgmentMode
	{
		/// <summary>
		/// Requests in the message queue will be automatically acknowledged by
		/// the OnPremiseConnector before executing the request against the on premise api
		/// </summary>
		Default,

		/// <summary>
		/// Requests in the message queue will be automatically acknowledged by
		/// the relay server before sending them to the OnPremiseConnector
		/// </summary>
		Auto,

		/// <summary>
		/// Requests in the message queue will not be acknowleged by the Relay system
		/// at all and have to be manually acknowleged by requesting the /request/acknowlegde endpoint
		/// with the connection and the acknowledge id
		/// </summary>
		Manual,
	}
}
