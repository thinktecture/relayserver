using Thinktecture.Relay.Interceptors;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayServerBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayServerBuilderExtensions
	{
		/// <summary>
		/// Adds the X-Forwarded header interceptor.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddForwardedHeaderInterceptor<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
		{
			builder.AddClientRequestInterceptor<TRequest, TResponse, ForwardedHeaderInterceptor<TRequest, TResponse>>();

			return builder;
		}
	}
}
