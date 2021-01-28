using System;

namespace Thinktecture.Relay.Server.Security
{
	/// <summary>
	/// Provides information about a tenant.
	/// </summary>
	public class TenantInfo
	{
		/// <summary>
		/// Gets or sets the id of the tenant.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the name of the tenant.
		/// </summary>
		public string Name { get; set; } = default!;

		/// <summary>
		/// Gets or sets the display name of the tenant.
		/// </summary>
		public string DisplayName { get; set; } = default!;
	}
}
