using System;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (same IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds a <see cref="RelayWebTarget{ClientRequest,TargetResponse}"/> to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<ClientRequest, TargetResponse> AddWebTarget(
			this IRelayConnectorBuilder<ClientRequest, TargetResponse> builder, string id, RelayWebTargetOptions options)
			=> builder.AddWebTarget<ClientRequest, TargetResponse>(id, options);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{ClientRequest,TargetResponse}"/> to the <see cref="IServiceCollection"/> as catch-all fallback.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<ClientRequest, TargetResponse> AddCatchAllWebTarget(
			this IRelayConnectorBuilder<ClientRequest, TargetResponse> builder, RelayWebTargetOptions options)
			=> builder.AddCatchAllWebTarget<ClientRequest, TargetResponse>(options);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddWebTarget<TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, string id, RelayWebTargetOptions options)
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
			=> builder.AddTarget<RelayWebTarget<TRequest, TResponse>, TRequest, TResponse>(id, options);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/> as catch-all fallback.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddCatchAllWebTarget<TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, RelayWebTargetOptions options)
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
			=> builder.AddTargetInternal<RelayWebTarget<TRequest, TResponse>, TRequest, TResponse>(Constants.RelayTargetCatchAllId, options);

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="options">The <see cref="IRelayTargetOptions"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddTarget<TTarget, TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, string id, IRelayTargetOptions options = null)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			if (id == Constants.RelayTargetCatchAllId)
			{
				throw new ArgumentOutOfRangeException(nameof(id), id, $"The value cannot be '{Constants.RelayTargetCatchAllId}'");
			}

			builder.AddTargetInternal<TTarget, TRequest, TResponse>(id, options);

			return builder;
		}

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/> as catch-all fallback.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="options">The <see cref="IRelayTargetOptions"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddCatchAllTarget<TTarget, TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, IRelayTargetOptions options = null)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			builder.AddTargetInternal<TTarget, TRequest, TResponse>(Constants.RelayTargetCatchAllId, options);

			return builder;
		}

		private static IRelayConnectorBuilder<TRequest, TResponse> AddTargetInternal<TTarget, TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, string id, IRelayTargetOptions options)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			builder.Services.AddSingleton(provider => new RelayTargetRegistration<TRequest, TResponse>(typeof(TTarget), id, options));

			return builder;
		}
	}
}
