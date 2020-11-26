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
		/// <remarks>The default value is 64 kb.</remarks>
		public int MaximumBinarySize { get; set; } = 64 * 1024; // 64kb
	}
}
