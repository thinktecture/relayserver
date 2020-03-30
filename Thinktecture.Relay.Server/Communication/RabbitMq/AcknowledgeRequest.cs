namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class AcknowledgeRequest : IAcknowledgeRequest
	{
		public string ConnectionId { get; set; }
		public string AcknowledgeId { get; set; }
	}
}
