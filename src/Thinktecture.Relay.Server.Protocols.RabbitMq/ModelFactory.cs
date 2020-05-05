using RabbitMQ.Client;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <summary>
	/// A factory implementation for creating <see cref="IModel"/> instances.
	/// </summary>
	public class ModelFactory
	{
		private readonly IConnection _connection;

		/// <summary>
		/// Initializes a new instance of <see cref="ModelFactory"/>.
		/// </summary>
		/// <param name="connection">The <see cref="IConnection"/>.</param>
		public ModelFactory(IConnection connection) => _connection = connection;

		/// <summary>
		/// Creates a new instance of <see cref="IModel"/>.
		/// </summary>
		/// <returns>An <see cref="IModel"/>.</returns>
		public IModel Create() => _connection.CreateModel();
	}
}
