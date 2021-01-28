using System;

namespace Thinktecture.Relay.Server.Persistence.Models
{
	/// <summary>
	/// Represents a single connection of tenants' on-premises installation to a relay server.
	/// </summary>
	public class Connection
	{
		/// <summary>
		/// The transport-specific connection id.
		/// </summary>
		/// <remarks>The maximum length is 100 unicode characters.</remarks>
		public string Id { get; set; } = default!;

		/// <summary>
		/// The unique id of the tenant.
		/// </summary>
		public Guid TenantId { get; set; }

		/// <summary>
		/// The unique id of the relay server instance this connection is held to.
		/// </summary>
		public Guid OriginId { get; set; }

		/// <summary>
		/// The time when this connection was opened.
		/// </summary>
		public DateTimeOffset ConnectTime { get; set; }

		/// <summary>
		/// The time when this connection was closed.
		/// </summary>
		public DateTimeOffset? DisconnectTime { get; set; }

		/// <summary>
		/// The time when the last message was sent or received through this connection.
		/// </summary>
		public DateTimeOffset? LastActivityTime { get; set; }

		/// <summary>
		/// The remote IP address of the connector.
		/// </summary>
		public string? RemoteIpAddress { get; set; }
	}
}
