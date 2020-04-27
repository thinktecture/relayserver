using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.Entities
{
	/// <summary>
	/// Represents a tenant. A tenant can have multiple connectors on one physical on-premises
	/// </summary>
	public class Tenant
	{
		/// <summary>
		/// The unique Id of the <see cref="Tenant"/>.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The name of the <see cref="Tenant"/>. Also used as ClientId for connector authentication.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The display name of the <see cref="Tenant"/>. Will be used as a visual identifier on the management UI.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// An optional, longer, textual description of this <see cref="Tenant"/>.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The client secrets, used for authentication connectors for this <see cref="Tenant"/>.
		/// </summary>
		public List<ClientSecret> ClientSecrets { get; set; }
	}
}
