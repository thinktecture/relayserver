using System;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> as the request timeout.</param>
		/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<ClientRequest, TargetResponse> AddTarget<TTarget>(
			this IRelayConnectorBuilder<ClientRequest, TargetResponse> builder, string id, TimeSpan? timeout = null,
			params object[] parameters)
			where TTarget : IRelayTarget<ClientRequest, TargetResponse>
			=> builder.AddTarget<TTarget, ClientRequest, TargetResponse>(id, timeout, parameters);

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> as the request timeout.</param>
		/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddTarget<TTarget, TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, string id, TimeSpan? timeout = null,
			params object[] parameters)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
		{
			builder.Services.Configure<RelayTargetServiceOptions<TRequest, TResponse>>(configure =>
			{
				configure.Registrations.Add(new RelayTargetRegistration<TTarget, TRequest, TResponse>(id, timeout, parameters));
			});

			return builder;
		}

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="factory">The factory function to create an instance of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> as the request timeout.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddTarget<TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, string id,
			Func<IServiceProvider, IRelayTarget<TRequest, TResponse>> factory, TimeSpan? timeout = null)
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			builder.Services.Configure<RelayTargetServiceOptions<TRequest, TResponse>>(configure =>
			{
				configure.Registrations.Add(new RelayTargetRegistration<TRequest, TResponse>(id, factory, timeout));
			});

			return builder;
		}
	}
}
