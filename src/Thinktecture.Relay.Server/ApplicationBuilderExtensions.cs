using Thinktecture.Relay.Abstractions;
using Thinktecture.Relay.Server.Middleware;

// ReSharper disable once CheckNamespace; (extension methods on IApplicationBuilder namespace)
namespace Microsoft.AspNetCore.Builder
{
	/// <summary>
	/// Extension methods for the <see cref="IApplicationBuilder"/>.
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds the <see cref="RelayMiddleware{ClientRequest}"/> to the application's request pipeline.
		/// </summary>
		/// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
		/// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
		public static IApplicationBuilder UseRelayServer(this IApplicationBuilder builder)
		{
			return builder.UseRelayServer<ClientRequest>();
		}

		/// <summary>
		/// Adds the <see cref="RelayMiddleware{TRequest}"/> to the application's request pipeline.
		/// </summary>
		/// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
		/// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
		public static IApplicationBuilder UseRelayServer<TRequest>(this IApplicationBuilder builder)
			where TRequest : ITransportClientRequest, new()
		{
			return builder.Map("/relay", app => app.UseMiddleware<RelayMiddleware<TRequest>>());
		}
	}
}
