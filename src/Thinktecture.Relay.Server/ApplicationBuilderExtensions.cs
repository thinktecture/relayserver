using Thinktecture.Relay.Server;

// ReSharper disable once CheckNamespace; (extension methods on IApplicationBuilder namespace)
namespace Microsoft.AspNetCore.Builder
{
	/// <summary>
	/// Extension methods for the <see cref="IApplicationBuilder"/>.
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds the <see cref="RelayServerMiddleware"/> to the application's request pipeline.
		/// </summary>
		/// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
		/// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
		public static IApplicationBuilder UseRelayServer(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RelayServerMiddleware>();
		}
	}
}
