namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

internal static class Constants
{
	public const string ExchangeName = "RelayServer";

	/// <summary>
	/// The prefix for request queues.
	/// </summary>
	/// <remarks>Should be followed by a space and the tenant name.</remarks>
	public const string RequestQueuePrefix = "Requests";

	/// <summary>
	/// The prefix for response queues.
	/// </summary>
	/// <remarks>Should be followed by a space and the origin id.</remarks>
	public const string ResponseQueuePrefix = "Responses";

	/// <summary>
	/// The prefix for acknowledge queues.
	/// </summary>
	/// <remarks>Should be followed by a space and the origin id.</remarks>
	public const string AcknowledgeQueuePrefix = "Acknowledges";
}
