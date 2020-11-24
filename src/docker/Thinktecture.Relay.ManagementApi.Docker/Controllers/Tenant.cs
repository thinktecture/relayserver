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
		public string Name { get; set; }

		/// <summary>
		/// The display name of the tenant. Will be used as a visual identifier on the management UI.
		/// </summary>
		/// <example>My on premise connector</example>
		public string DisplayName { get; set; }

		/// <summary>
		/// An optional, longer, textual description of this tenant.
		/// </summary>
		/// <example>On premise connector in the Thinktecture office in Karlsruhe</example>
		public string Description { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Tenant"/> class.
		/// </summary>
		/// <remarks>Parameterless constructor is needed for deserialization</remarks>
		public Tenant() {}

		/// <summary>
		/// Initializes a new instance of the <see cref="Tenant"/> class.
		/// </summary>
		/// <param name="tenant">The <see cref="Thinktecture.Relay.Server.Persistence.Models.Tenant"/> to clone.</param>
		public Tenant(Thinktecture.Relay.Server.Persistence.Models.Tenant tenant)
		{
			Id = tenant.Id;
			Name = tenant.Name;
			DisplayName = tenant.DisplayName;
			Description = tenant.Description;
		}
	}

	internal static class TenantExtensions
	{
		public static Tenant ToTenantModel(this Thinktecture.Relay.Server.Persistence.Models.Tenant tenant)
		{
			return new Tenant(tenant);
		}

		public static async IAsyncEnumerable<Tenant> ToTenantModels(this IAsyncEnumerable<Thinktecture.Relay.Server.Persistence.Models.Tenant> tenants)
		{
			await foreach (var tenant in tenants)
				yield return tenant.ToTenantModel();
		}

		public static Thinktecture.Relay.Server.Persistence.Models.Tenant ToTenant(this Tenant tenant)
		{
			return new Thinktecture.Relay.Server.Persistence.Models.Tenant()
			{
				Id = tenant.Id,
				Name = tenant.Name,
				DisplayName = tenant.DisplayName,
				Description = tenant.Description,
			};
		}
	}
}
