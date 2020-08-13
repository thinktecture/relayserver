using Microsoft.AspNetCore.Builder;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class ApplicationBuilderPart<TRequest, TResponse> : IApplicationBuilderPart
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <inheritdoc />
		public void Use(IApplicationBuilder builder)
			=> builder.UseEndpoints(endpoints => endpoints.MapHub<ConnectorHub<TRequest, TResponse>>("/connector"));
	}
}
