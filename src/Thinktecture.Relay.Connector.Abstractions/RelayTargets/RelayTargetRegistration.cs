using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <summary>
	/// Represents a registration information associated with an <see cref="IRelayTarget{TRequest,TResponse}"/>.
	/// </summary>
	public class RelayTargetRegistration<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// The unique id of the target.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// A factory function to create an instance of the target.
		/// </summary>
		public Func<IServiceProvider, IRelayTarget<TRequest, TResponse>> Factory { get; }

		/// <summary>
		/// Gets the <see cref="TimeSpan"/> to wait before the request to the target times out.
		/// </summary>
		/// <remarks>The default value is 100 seconds.</remarks>
		public TimeSpan Timeout { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayTargetRegistration{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="factory">The factory function to create an instance of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> as the request timeout.</param>
		public RelayTargetRegistration(string id, Func<IServiceProvider, IRelayTarget<TRequest, TResponse>> factory, TimeSpan? timeout = null)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			Timeout = timeout ?? TimeSpan.FromSeconds(100);
		}
	}

	/// <inheritdoc />
	public class RelayTargetRegistration<TRequest, TResponse, TTarget> : RelayTargetRegistration<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
		where TTarget : IRelayTarget<TRequest, TResponse>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RelayTargetRegistration{TRequest,TResponse,TTarget}"/> class.
		/// </summary>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> as the request timeout.</param>
		public RelayTargetRegistration(string id, TimeSpan? timeout = null, params object[] parameters)
			: base(id, provider => ActivatorUtilities.CreateInstance<TTarget>(provider, parameters), timeout)
		{
		}
	}
}
