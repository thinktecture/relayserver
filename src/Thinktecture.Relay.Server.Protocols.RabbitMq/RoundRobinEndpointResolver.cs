using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc />
	public class RoundRobinEndpointResolver : IEndpointResolver
	{
		private readonly AmqpTcpEndpoint[] _endpoints;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoundRobinEndpointResolver"/> class.
		/// </summary>
		/// <param name="endpoints">The <see cref="AmqpTcpEndpoint"/>s to use.</param>
		public RoundRobinEndpointResolver(IEnumerable<AmqpTcpEndpoint> endpoints)
			=> _endpoints = endpoints?.ToArray() ?? throw new ArgumentNullException(nameof(endpoints));

		/// <inheritdoc />
		public IEnumerable<AmqpTcpEndpoint> All() => _endpoints;
	}
}
