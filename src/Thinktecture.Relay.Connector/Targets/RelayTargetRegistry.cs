using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <summary>
/// A registry for <see cref="IRelayTarget"/> types.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public partial class RelayTargetRegistry<TRequest, TResponse>
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
{
	private readonly ILogger _logger;

	private readonly ConcurrentDictionary<string, RelayTargetRegistration> _targets =
		new ConcurrentDictionary<string, RelayTargetRegistration>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayTargetRegistry{TRequest,TResponse}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="relayTargetOptions">An <see cref="IOptions{TOptions}"/>.</param>
	public RelayTargetRegistry(ILogger<RelayTargetRegistry<TRequest, TResponse>> logger,
		IOptions<RelayTargetOptions> relayTargetOptions)
	{
		if (relayTargetOptions is null) throw new ArgumentNullException(nameof(relayTargetOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		foreach (var target in relayTargetOptions.Value.Targets)
		{
			Register(target.Id, target.Type, target.Timeout, target.Parameters);
		}
	}

	/// <summary>
	/// Registers an <see cref="IRelayTarget"/> type.
	/// </summary>
	/// <param name="id">The unique id of the target.</param>
	/// <param name="type">The <see cref="Type"/> of the target.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
	/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
	/// <exception cref="ArgumentException">A registration with the same key already exists.</exception>
	public void Register(string id, Type type, TimeSpan? timeout = null, params object[] parameters)
	{
		var registration = new RelayTargetRegistration(
			provider => (IRelayTarget)ActivatorUtilities.CreateInstance(provider, type, parameters),
			timeout);

		if (!_targets.TryAdd(id, registration))
			throw new ArgumentException($"A registration with the same key \"{id}\" already exists", nameof(id));

		Log.RegisteredTarget(_logger, id, type.FullName);
	}

	/// <summary>
	/// Registers an <see cref="IRelayTarget"/> type.
	/// </summary>
	/// <param name="id">The unique id of the target.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
	/// <param name="parameters">Constructor arguments not provided by the <see cref="IServiceProvider"/>.</param>
	/// <typeparam name="T">The type of target.</typeparam>
	public void Register<T>(string id, TimeSpan? timeout = null, params object[] parameters)
		where T : IRelayTarget
		=> Register(id, typeof(T), timeout, parameters);

	/// <summary>
	/// Unregisters an <see cref="IRelayTarget"/>.
	/// </summary>
	/// <param name="id">The unique id of the target.</param>
	/// <remarks>This method does not fail when the relay target was not registered.</remarks>
	public void Unregister(string id)
	{
		if (_targets.TryRemove(id, out _))
		{
			Log.UnregisteredTarget(_logger, id);
		}
		else
		{
			Log.CouldNotUnregisterTarget(_logger, id);
		}
	}

	/// <summary>
	/// Attempts to create an <see cref="IRelayTarget"/> instance from the specified id.
	/// </summary>
	/// <param name="id">The unique id of the target.</param>
	/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
	/// <param name="target">
	/// When this method returns, <paramref name="target"/> contains the
	/// <see cref="IRelayTarget"/> or null, if the target was not found.
	/// </param>
	/// <param name="timeout">
	/// When this method returns, <paramref name="timeout"/> contains the
	/// <see cref="CancellationTokenSource"/> or null, if the target was not found.
	/// </param>
	/// <returns>true if the target was found; otherwise, false.</returns>
	internal bool TryCreateRelayTarget(string id, IServiceProvider serviceProvider,
		[MaybeNullWhen(false)] out IRelayTarget target,
		[MaybeNullWhen(false)] out CancellationTokenSource timeout)
	{
		if (_targets.TryGetValue(id, out var registration) ||
		    _targets.TryGetValue(Constants.RelayTargetCatchAllId, out registration))
		{
			target = registration.Factory(serviceProvider);
			timeout = new CancellationTokenSource(registration.Timeout);
			return true;
		}

		target = null;
		timeout = default;
		return false;
	}

	private class RelayTargetRegistration
	{
		public Func<IServiceProvider, IRelayTarget> Factory { get; }

		public TimeSpan Timeout { get; }

		public RelayTargetRegistration(Func<IServiceProvider, IRelayTarget> factory,
			TimeSpan? timeout = null)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			Timeout = timeout ?? RelayTargetOptions.DefaultTargetTimeout;
		}
	}
}
