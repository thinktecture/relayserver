namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	internal static class Constants
	{
		public const string ExchangeName = "Relay Server";

		/// <summary>
		/// The prefix (including trailing white-space) for request queues.
		/// </summary>
		/// <remarks>Should be followed by a tenant id.</remarks>
		public const string RequestQueuePrefix = "Requests ";

		/// <summary>
		/// The prefix (including trailing white-space) for response queues.
		/// </summary>
		/// <remarks>Should be followed by an origin id.</remarks>
		public const string ResponseQueuePrefix = "Responses ";

		/// <summary>
		/// The prefix (including trailing white-space) for acknowledge queues.
		/// </summary>
		/// <remarks>Should be followed by an origin id.</remarks>
		public const string AcknowledgeQueuePrefix = "Acknowledges ";
	}
}
