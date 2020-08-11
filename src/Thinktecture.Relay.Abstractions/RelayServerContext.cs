using System;

namespace Thinktecture.Relay
{
	/// <summary>
	/// Provides contextual information for this RelayServer instance.
	/// </summary>
	public class RelayServerContext
	{
		/// <summary>
		/// The unique id of this instance of RelayServer.
		/// </summary>
		public Guid OriginId { get; } = Guid.NewGuid();
	}
}
