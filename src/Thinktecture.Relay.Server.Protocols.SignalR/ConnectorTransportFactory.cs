using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class ConnectorTransportFactory<TRequest, TResponse, TAcknowledge> : IConnectorTransportFactory<TRequest>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectorTransportFactory{TRequest,TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public ConnectorTransportFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		/// <inheritdoc />
		public IConnectorTransport<TRequest> Create(string connectionId)
			=> ActivatorUtilities.CreateInstance<ConnectorTransport<TRequest, TResponse, TAcknowledge>>(_serviceProvider, connectionId);
	}
}
