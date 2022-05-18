using System;

namespace Thinktecture.Relay.Server;

/// <summary>
/// Provides contextual information.
/// </summary>
public class RelayServerContext
{
	/// <summary>
	/// The unique id of this instance.
	/// </summary>
	/// <remarks>This changes during every startup.</remarks>
	public Guid OriginId { get; } = Guid.NewGuid();
}
