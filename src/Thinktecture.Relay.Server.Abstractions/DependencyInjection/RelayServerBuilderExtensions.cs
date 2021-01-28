using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Interceptor;
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
		/// Adds the <see cref="IClientRequestInterceptor{ClientRequest,TargetResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/> instance.</param>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/> instance.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> AddClientRequestInterceptor<TInterceptor>(
			this IRelayServerBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> builder)
			where TInterceptor : class, IClientRequestInterceptor<ClientRequest, TargetResponse>
		{
			builder.AddClientRequestInterceptor<ClientRequest, TargetResponse, AcknowledgeRequest, TInterceptor>();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="IClientRequestInterceptor{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <typeparam name="TAcknowledge">THe type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse, TAcknowledge> AddClientRequestInterceptor<TRequest, TResponse, TAcknowledge,
			TInterceptor>(this IRelayServerBuilder<TRequest, TResponse, TAcknowledge> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
			where TInterceptor : class, IClientRequestInterceptor<TRequest, TResponse>
			where TAcknowledge : IAcknowledgeRequest
		{
			builder.Services.AddScoped<IClientRequestInterceptor<TRequest, TResponse>, TInterceptor>();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="ITargetResponseInterceptor{ClientRequest,TargetResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/> instance.</param>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/> instance.</returns>
		public static IRelayServerBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> AddTargetResponseInterceptor<TInterceptor>(
			this IRelayServerBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> builder)
			where TInterceptor : class, ITargetResponseInterceptor<ClientRequest, TargetResponse>
		{
			builder.Services.AddScoped<ITargetResponseInterceptor<ClientRequest, TargetResponse>, TInterceptor>();

			return builder;
		}

		/// <summary>
		/// Adds the <see cref="ITargetResponseInterceptor{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TInterceptor">The type of interceptor.</typeparam>
		/// <typeparam name="TAcknowledge">THe type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse,TAcknowledge}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse, TAcknowledge> AddTargetResponseInterceptor<TRequest, TResponse, TAcknowledge,
			TInterceptor>(this IRelayServerBuilder<TRequest, TResponse, TAcknowledge> builder)
			where TRequest : IClientRequest
			where TResponse : class, ITargetResponse
			where TInterceptor : class, ITargetResponseInterceptor<TRequest, TResponse>
			where TAcknowledge : IAcknowledgeRequest
		{
			builder.Services.AddScoped<ITargetResponseInterceptor<TRequest, TResponse>, TInterceptor>();

			return builder;
		}
	}
}
