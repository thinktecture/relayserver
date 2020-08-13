using RabbitMQ.Client;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <summary>
	/// Configuration options for Rabbit MQ.
	/// </summary>
	public class RabbitMqOptions
	{
		/// <summary>
		/// The Uri to connect to.
		/// </summary>
		public string Uri { get; set; }

		/// <summary>
		/// The comma-separated hosts of a Rabbit MQ cluster.
		/// </summary>
		public string ClusterHosts { get; set; }

		/// <summary>
		/// The maximum size for inline binary data in a Rabbit MQ message.
		/// </summary>
		public int MaximumBinarySize { get; set; } = 64 * 1024; // 64kb
	}
}
