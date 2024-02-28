using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <summary>
/// An implementation of a target.
/// </summary>
/// <remarks>This is just a marker interface.</remarks>
public interface IRelayTarget
{
}

/// <summary>
/// An implementation of a target executing logic triggered by a request.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
public interface IRelayTargetAction<in TRequest> : IRelayTarget
	where TRequest : IClientRequest
{
	/// <summary>
	/// Called when the target should be executed.
	/// </summary>
	/// <param name="request">The client request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public interface IRelayTargetAction : IRelayTargetAction<ClientRequest>
{
}

/// <summary>
/// An implementation of a target providing the necessary response information for a request.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IRelayTargetFunc<in TRequest, TResponse> : IRelayTarget
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
{
	/// <summary>
	/// Called when the target should be requested.
	/// </summary>
	/// <param name="request">The client request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the target response.</returns>
	Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public interface IRelayTargetFunc : IRelayTargetFunc<ClientRequest, TargetResponse>
{
}
