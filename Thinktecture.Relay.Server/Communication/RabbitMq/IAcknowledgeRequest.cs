namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	public interface IAcknowledgeRequest
	{
		string AcknowledgeId { get; }
		string ConnectionId { get; }
	}
}
