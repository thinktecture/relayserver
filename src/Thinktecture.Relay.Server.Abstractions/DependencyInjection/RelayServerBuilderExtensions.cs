using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Interceptor;
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
		/// Adds the <see cref="IClientRequestInterceptor{ClientRequest,TargetResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse}"/> instance.</param>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse}"/> instance.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse> AddClientRequestInterceptor<TInterceptor>(
			this IRelayServerBuilder<ClientRequest, TargetResponse> builder)
			where TInterceptor : class, IClientRequestInterceptor<ClientRequest, TargetResponse>
		{
			builder.AddClientRequestInterceptor<ClientRequest, TargetResponse, TInterceptor>();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="IClientRequestInterceptor{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddClientRequestInterceptor<TRequest, TResponse, TInterceptor>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
			where TInterceptor : class, IClientRequestInterceptor<TRequest, TResponse>
		{
			builder.Services.AddScoped<IClientRequestInterceptor<TRequest, TResponse>, TInterceptor>();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="ITargetResponseInterceptor{ClientRequest,TargetResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse}"/> instance.</param>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse}"/> instance.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse> AddTargetResponseInterceptor<TInterceptor>(
			this IRelayServerBuilder<ClientRequest, TargetResponse> builder)
			where TInterceptor : class, ITargetResponseInterceptor<ClientRequest, TargetResponse>
		{
			builder.Services.AddScoped<ITargetResponseInterceptor<ClientRequest, TargetResponse>, TInterceptor>();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="ITargetResponseInterceptor{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddTargetResponseInterceptor<TRequest, TResponse, TInterceptor>(
			this IRelayServerBuilder<TRequest, TResponse> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
			where TInterceptor : class, ITargetResponseInterceptor<TRequest, TResponse>
		{
			builder.Services.AddScoped<ITargetResponseInterceptor<TRequest, TResponse>, TInterceptor>();

			return builder;
		}
	}
}
