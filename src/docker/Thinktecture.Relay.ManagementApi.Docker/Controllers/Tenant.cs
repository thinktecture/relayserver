using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Thinktecture.Relay.ManagementApi.Docker.Controllers
{
	/// <summary>
	/// Represents a tenant. A tenant can have multiple connectors on one physical on-premises
	/// </summary>
	public class Tenant
	{
		/// <summary>
		/// The unique id of the tenant.
		/// </summary>
		/// <example>a8cda2aa-4e8b-4506-ba87-f150906cddbe</example>
		public Guid Id { get; set; }

		/// <summary>
		/// The name of the tenant. Also used as ClientId for connector authentication.
		/// </summary>
		/// <example>MyOnPremiseConnector</example>
		[Required]
		public string Name { get; set; } = default!;

		/// <summary>
		/// The display name of the tenant. Will be used as a visual identifier on the management UI.
		/// </summary>
		/// <example>My on premise connector</example>
		public string? DisplayName { get; set; }

		/// <summary>
		/// An optional, longer, textual description of this tenant.
		/// </summary>
		/// <example>On premise connector in the Thinktecture office in Karlsruhe</example>
		public string? Description { get; set; }
	}

	internal static class TenantExtensions
	{
		public static Tenant ToModel(this Thinktecture.Relay.Server.Persistence.Models.Tenant tenant)
		{
			return new Tenant()
			{
				Id = tenant.Id,
				Name = tenant.Name,
				DisplayName = tenant.DisplayName,
				Description = tenant.Description
			};
		}

		public static async IAsyncEnumerable<Tenant> ToModels(this IAsyncEnumerable<Thinktecture.Relay.Server.Persistence.Models.Tenant> tenants)
		{
			await foreach (var tenant in tenants)
			{
				yield return tenant.ToModel();
			}
		}

		public static Thinktecture.Relay.Server.Persistence.Models.Tenant ToTenant(this Tenant tenant)
		{
			return new Thinktecture.Relay.Server.Persistence.Models.Tenant()
			{
				Id = tenant.Id,
				Name = tenant.Name,
				DisplayName = tenant.DisplayName,
				Description = tenant.Description
			};
		}
	}
}
