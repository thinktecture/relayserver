using Microsoft.AspNetCore.Builder;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class ApplicationBuilderPart<TRequest, TResponse, TAcknowledge> : IApplicationBuilderPart
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		/// <inheritdoc />
		public void Use(IApplicationBuilder builder)
			=> builder.UseEndpoints(endpoints => endpoints.MapHub<ConnectorHub<TRequest, TResponse, TAcknowledge>>("/connector"));
	}
}
