using System.Collections.Generic;
using System.Linq;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;
using TenantModel = Thinktecture.Relay.Server.Management.DataTransferObjects.Tenant;
using TenantEntity = Thinktecture.Relay.Server.Persistence.Models.Tenant;
using ConfigEntity = Thinktecture.Relay.Server.Persistence.Models.Config;

namespace Thinktecture.Relay.Server.Management.Extensions;

/// <summary>
/// Provides extension methods to the persistence model types.
/// </summary>
internal static class PersistenceModelsExtensions
{
	/// <summary>
	/// Converts a <see cref="Thinktecture.Relay.Server.Persistence.Models.Tenant"/> to an corresponding
	/// <see cref="DataTransferObjects.Tenant"/>.
	/// </summary>
	/// <param name="tenant">The tenant instance to convert.</param>
	/// <returns>A new instance of an <see cref="DataTransferObjects.Tenant"/>.</returns>
	public static TenantModel ToModel(this TenantEntity tenant)
		=> new TenantModel()
		{
			// main tenant properties
			Name = tenant.Name,
			DisplayName = tenant.DisplayName,
			Description = tenant.Description,
			RequireAuthentication = tenant.RequireAuthentication,
			MaximumConcurrentConnectorRequests = tenant.MaximumConcurrentConnectorRequests,

			// config properties
			KeepAliveInterval = tenant.Config?.KeepAliveInterval,
			EnableTracing = tenant.Config?.EnableTracing,
			ReconnectMinimumDelay = tenant.Config?.ReconnectMinimumDelay,
			ReconnectMaximumDelay = tenant.Config?.ReconnectMaximumDelay,
		};

	/// <summary>
	/// Converts a <see cref="DataTransferObjects.Tenant"/> to an corresponding
	/// <see cref="Thinktecture.Relay.Server.Persistence.Models.Tenant"/>.
	/// </summary>
	/// <param name="tenant">The tenant instance to convert.</param>
	/// <returns>A new instance of an <see cref="Thinktecture.Relay.Server.Persistence.Models.Tenant"/>.</returns>
	public static TenantEntity ToEntity(this TenantModel tenant)
	{
		var config = (ConfigEntity?)null;

		if (tenant.KeepAliveInterval is not null ||
			tenant.EnableTracing is not null ||
			tenant.ReconnectMinimumDelay is not null ||
			tenant.ReconnectMaximumDelay is not null)
		{
			config = new ConfigEntity()
			{
				KeepAliveInterval = tenant.KeepAliveInterval,
				EnableTracing = tenant.EnableTracing,
				ReconnectMinimumDelay = tenant.ReconnectMinimumDelay,
				ReconnectMaximumDelay = tenant.ReconnectMaximumDelay,
			};
		}

		var entity = new TenantEntity()
		{
			Name = tenant.Name,
			DisplayName = tenant.DisplayName,
			Description = tenant.Description,
			Config = config,
		};

		if (tenant.RequireAuthentication.HasValue)
		{
			entity.RequireAuthentication = tenant.RequireAuthentication.Value;
		}

		if (tenant.MaximumConcurrentConnectorRequests.HasValue)
		{
			entity.MaximumConcurrentConnectorRequests = tenant.MaximumConcurrentConnectorRequests.Value;
		}

		return entity;
	}

	/// <summary>
	/// Converts an enumerable of <see cref="Thinktecture.Relay.Server.Persistence.Models.Tenant"/> to an
	/// enumerable of <see cref="DataTransferObjects.Tenant"/> by converting each
	/// entry individually.
	/// </summary>
	/// <param name="tenants">The tenants to convert.</param>
	/// <returns>The converted tenants.</returns>
	public static IEnumerable<TenantModel> ToModels(this IEnumerable<TenantEntity> tenants)
		=> tenants.Select(ToModel);

	/// <summary>
	/// Converts a paged result of <see cref="Thinktecture.Relay.Server.Persistence.Models.Tenant"/> to
	/// an paged result of <see cref="DataTransferObjects.Tenant"/>.
	/// </summary>
	/// <param name="page">The page to convert.</param>
	/// <returns>The converted page.</returns>
	public static Page<TenantModel> ToModel(this Page<TenantEntity> page)
		=> new Page<TenantModel>()
		{
			TotalCount = page.TotalCount,
			Offset = page.Offset,
			PageSize = page.PageSize,
			Results = page.Results.ToModels().ToArray(),
		};
}
