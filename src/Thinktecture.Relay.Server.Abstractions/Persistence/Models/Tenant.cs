using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Server.Persistence.Models
{
	/// <summary>
	/// Represents a tenant. A tenant can have multiple connectors on one physical on-premises
	/// </summary>
	public class Tenant
	{
		/// <summary>
		/// The unique id of the tenant.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The name of the tenant. Also used as ClientId for connector authentication.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The display name of the tenant. Will be used as a visual identifier on the management UI.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// An optional, longer, textual description of this tenant.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The client secrets, used for authentication connectors for this tenant.
		/// </summary>
		[JsonIgnore]
		public List<ClientSecret> ClientSecrets { get; set; }

		/// <summary>
		/// The normalized (e.g. ToUpperInvariant()) name of the tenant. Use this for case-insensitive comparison in the database.
		/// </summary>
		[JsonIgnore]
		public string NormalizedName { get; set; }
	}
}
