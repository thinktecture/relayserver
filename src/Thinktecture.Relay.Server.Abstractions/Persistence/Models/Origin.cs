using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Server.Persistence.Models
{
	/// <summary>
	/// Represents a relay server instance.
	/// </summary>
	public class Origin
	{
		/// <summary>
		/// The unique id of the running instance.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The time when this instance started.
		/// </summary>
		public DateTimeOffset StartupTime { get; set; }

		/// <summary>
		/// The time when this instance shut down.
		/// </summary>
		public DateTimeOffset? ShutdownTime { get; set; }

		/// <summary>
		/// The last time when this instance said it is still alive.
		/// </summary>
		public DateTimeOffset LastSeenTime { get; set; }

		/// <summary>
		/// All connections that are currently registered for this origin.
		/// </summary>
		[JsonIgnore]
		public List<Connection>? Connections { get; set; }
	}
}
