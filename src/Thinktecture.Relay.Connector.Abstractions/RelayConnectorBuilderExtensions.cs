using System;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Connector.Options;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds an <see cref="IRelayTarget{ClientRequest,TargetResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
		/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{ClientRequest,TargetResponse,AcknowledgeRequest}"/>.</returns>
		public static IRelayConnectorBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> AddTarget<TTarget>(
			this IRelayConnectorBuilder<ClientRequest, TargetResponse, AcknowledgeRequest> builder, string id, TimeSpan? timeout = null,
			params object[] parameters)
			where TTarget : IRelayTarget<ClientRequest, TargetResponse>
			=> builder.AddTarget<ClientRequest, TargetResponse, AcknowledgeRequest, TTarget>(id, timeout, parameters);

		/// <summary>
		/// Adds an <see cref="IRelayTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
		/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TTarget">The type of target.</typeparam>
		/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> AddTarget<TRequest, TResponse, TAcknowledge, TTarget>(
			this IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> builder, string id, TimeSpan? timeout = null,
			params object[] parameters)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
			where TTarget : IRelayTarget<TRequest, TResponse>
			where TAcknowledge : IAcknowledgeRequest
		{
			builder.Services.Configure<RelayTargetOptions>(options => options.Targets.Add(new RelayTargetOptions.RelayTargetRegistration
				{ Id = id, Type = typeof(TTarget), Timeout = timeout, Parameters = parameters }));

			return builder;
		}
	}
}
