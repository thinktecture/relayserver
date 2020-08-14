using System;
using System.Runtime.CompilerServices;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (same IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds a <see cref="RelayWebTarget{ClientRequest,TargetResponse}"/> to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddWebTarget(this IRelayConnectorBuilder builder, string id, RelayWebTargetOptions options)
			=> builder.AddWebTarget<ClientRequest, TargetResponse>(id, options);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{ClientRequest,TargetResponse}"/> to the <see cref="IServiceCollection"/> as catch-all fallback.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder"/>.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddCatchAllWebTarget(this IRelayConnectorBuilder builder, RelayWebTargetOptions options)
			=> builder.AddCatchAllWebTarget<ClientRequest, TargetResponse>(options);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddWebTarget<TRequest, TResponse>(this IRelayConnectorBuilder builder, string id,
			RelayWebTargetOptions options)
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
			=> builder.AddTarget<RelayWebTarget<TRequest, TResponse>, TRequest, TResponse>(id, options);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/> as catch-all fallback.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder"/>.</param>
		/// <param name="options">The <see cref="RelayWebTargetOptions"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddCatchAllWebTarget<TRequest, TResponse>(this IRelayConnectorBuilder builder,
			RelayWebTargetOptions options)
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
			=> builder.AddTargetInternal<RelayWebTarget<TRequest, TResponse>, TRequest, TResponse>(
				RelayConnectorBuilder.RelayTargetCatchAllId,
				options);

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="options">The <see cref="IRelayTargetOptions"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddTarget<TTarget, TRequest, TResponse>(this IRelayConnectorBuilder builder, string id,
			IRelayTargetOptions options = null)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			if (id == RelayConnectorBuilder.RelayTargetCatchAllId)
			{
				throw new ArgumentOutOfRangeException(nameof(id), id, $"The value cannot be '{RelayConnectorBuilder.RelayTargetCatchAllId}'");
			}

			builder.AddTargetInternal<TTarget, TRequest, TResponse>(id, options);

			return builder;
		}

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/> to the <see cref="IServiceCollection"/> as catch-all fallback.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder"/>.</param>
		/// <param name="options">The <see cref="IRelayTargetOptions"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder"/>.</returns>
		public static IRelayConnectorBuilder AddCatchAllTarget<TTarget, TRequest, TResponse>(this IRelayConnectorBuilder builder,
			IRelayTargetOptions options = null)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			builder.AddTargetInternal<TTarget, TRequest, TResponse>(RelayConnectorBuilder.RelayTargetCatchAllId, options);

			return builder;
		}

		private static IRelayConnectorBuilder AddTargetInternal<TTarget, TRequest, TResponse>(this IRelayConnectorBuilder builder, string id,
			IRelayTargetOptions options)
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TResponse : ITargetResponse
			where TRequest : IClientRequest
		{
			builder.Services.Configure<RelayConnectorOptions<TRequest, TResponse>>(
				connector => connector.Targets[id] = new RelayTargetRegistration<TRequest, TResponse>(options, typeof(TTarget)));

			return builder;
		}
	}
}
