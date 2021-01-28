using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Interceptors;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/>.
	/// </summary>
	public static class RelayServerBuilderExtensions
	{
		/// <summary>
		/// Adds the X-Forwarded header interceptor.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse, TAcknowledge> AddForwardedHeaderInterceptor<TRequest, TResponse, TAcknowledge>(
			this IRelayServerBuilder<TRequest, TResponse, TAcknowledge> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
			where TAcknowledge : IAcknowledgeRequest
		{
			builder.AddClientRequestInterceptor<TRequest, TResponse, TAcknowledge, ForwardedHeaderInterceptor<TRequest, TResponse>>();

			return builder;
		}
	}
}
