using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Thinktecture.Relay.Server.Management.DataTransferObjects;
using Thinktecture.Relay.Server.Persistence.DataTransferObjects;
using Thinktecture.Relay.Server.Persistence.Models;
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

			// config properties
			KeepAliveInterval = tenant.Config?.KeepAliveInterval,
			EnableTracing = tenant.Config?.EnableTracing,
			ReconnectMinimumDelay = tenant.Config?.ReconnectMinimumDelay,
			ReconnectMaximumDelay = tenant.Config?.ReconnectMaximumDelay,

			// credential property
			Credentials = tenant.ClientSecrets?.Select(s => new TenantCredential()
				{
					Id = s.Id,
					Created = s.Created,
					Expiration = s.Expiration,
				}).ToArray() ??
				Array.Empty<TenantCredential>(),
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

		if (tenant.KeepAliveInterval != null ||
			tenant.EnableTracing != null ||
			tenant.ReconnectMinimumDelay != null ||
			tenant.ReconnectMaximumDelay != null)
		{
			config = new ConfigEntity()
			{
				KeepAliveInterval = tenant.KeepAliveInterval,
				EnableTracing = tenant.EnableTracing,
				ReconnectMinimumDelay = tenant.ReconnectMinimumDelay,
				ReconnectMaximumDelay = tenant.ReconnectMaximumDelay,
			};
		}

		return new TenantEntity()
		{
			Name = tenant.Name,
			DisplayName = tenant.DisplayName,
			Description = tenant.Description,
			Config = config,
			ClientSecrets = tenant.Credentials.Select(s =>
				new ClientSecret()
				{
					Id = s.Id,
					Created = s.Created,
					Value = s.Value ?? Sha512(s.PlainTextValue)!,
					Expiration = s.Expiration,
				}
			).ToList(),
		};
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

	private static string? Sha512(string? input)
	{
		if (String.IsNullOrEmpty(input)) return null;

		using var sha = SHA512.Create();
		var bytes = Encoding.UTF8.GetBytes(input);
		var hash = sha.ComputeHash(bytes);

		return Convert.ToBase64String(hash);
	}
}
