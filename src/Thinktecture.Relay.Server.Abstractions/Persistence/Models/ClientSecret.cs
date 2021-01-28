using System;

namespace Thinktecture.Relay.Server.Persistence.Models
{
	/// <summary>
	/// Represents a client secret which a connector for a <see cref="Tenant"/> needs to use for authentication.
	/// </summary>
	public class ClientSecret
	{
		/// <summary>
		/// The unique id of this client secret.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The unique id of the <see cref="Tenant"/> this secret is for.
		/// </summary>
		public Guid TenantId { get; set; }

		/// <summary>
		/// A SHA256 or SHA512 of the actual secret string.
		/// </summary>
		/// <remarks>The maximum length is 4000 unicode characters.</remarks>
		public string Value { get; set; } = default!;

		/// <summary>
		/// Defines an optional point in time when this secret automatically will become invalid.
		/// </summary>
		public DateTime? Expiration { get; set; }

		/// <summary>
		/// Indicates the point in time when this secret was created.
		/// </summary>
		public DateTime Created { get; set; }
	}
}
