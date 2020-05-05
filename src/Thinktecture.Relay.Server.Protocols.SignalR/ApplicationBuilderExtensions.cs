using Thinktecture.Relay.Server.Protocols.SignalR;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IApplicationBuilder namespace)
namespace Microsoft.AspNetCore.Builder
{
	/// <summary>
	/// Extension methods for the <see cref="IApplicationBuilder"/>.
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds the <see cref="ConnectorHub{TRequest,TResponse}"/> to the application's request pipeline.
		/// </summary>
		/// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
		/// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
		public static IApplicationBuilder UseSignalRTransport(this IApplicationBuilder builder)
			=> builder.UseSignalRTransport<ClientRequest, TargetResponse>();

		/// <summary>
		/// Adds the <see cref="ConnectorHub{TRequest,TResponse}"/> to the application's request pipeline.
		/// </summary>
		/// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
		public static IApplicationBuilder UseSignalRTransport<TRequest, TResponse>(this IApplicationBuilder builder)
			where TRequest : IRelayClientRequest
			where TResponse : IRelayTargetResponse
			=> builder.UseSignalR(hubRouteBuilder => hubRouteBuilder.MapHub<ConnectorHub<TRequest, TResponse>>("/connector"));
	}
}
