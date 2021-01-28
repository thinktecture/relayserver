using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.ManagementApi.Docker.Controllers
{
	/// <summary>
	/// Manages tenants.
	/// </summary>
	[AllowAnonymous] // TODO Authentication
	[Route("api/[controller]")]
	public class TenantController : Controller
	{
		private readonly ITenantRepository _tenantRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantController"/> class.
		/// </summary>
		/// <param name="tenantRepository">An instance of an <see cref="ITenantRepository"/>.</param>
		public TenantController(ITenantRepository tenantRepository)
			=> _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));

		/// <summary>
		/// Returns tenants in a pageable way.
		/// </summary>
		/// <param name="skip">The amount of tenants to skip while loading.</param>
		/// <param name="take">The amount of tenants to return. The default value is 10.</param>
		/// <returns>A page that contains <paramref name="take"/> tenants, starting at the <paramref name="skip"/> tenant.</returns>
		[HttpGet]
		public IAsyncEnumerable<Tenant> GetAllTenants(int skip = 0, int take = 10)
			=> _tenantRepository.LoadAllTenantsPagedAsync(skip, take).ToModels();

		/// <summary>
		/// Finds a tenant by its id.
		/// </summary>
		/// <param name="tenantId">The id of the tenant to load.</param>
		/// <returns>A tenant if found; otherwise 404.</returns>
		[HttpGet("{tenantId:guid}")]
		[SwaggerResponse(200, "Tenant with given id.", typeof(Tenant))]
		[SwaggerResponse(404, "Tenant with given id was not found.")]
		public async Task<ActionResult<Tenant>> GetTenantById([FromRoute] Guid tenantId)
		{
			var tenant = await _tenantRepository.LoadTenantByIdAsync(tenantId);
			if (tenant == null)
			{
				return NotFound();
			}

			return Ok(tenant.ToModel());
		}

		/// <summary>
		/// Finds a tenant by its name.
		/// </summary>
		/// <param name="tenantName">The name of the tenant to load.</param>
		/// <returns>A tenant if found; otherwise 404.</returns>
		[HttpGet("{tenantName}")]
		[SwaggerResponse(200, "Tenant with given name.", typeof(Tenant))]
		[SwaggerResponse(404, "Tenant with given name was not found.")]
		public async Task<ActionResult<Tenant>> GetTenantByName([FromRoute] string tenantName)
		{
			var tenant = await _tenantRepository.LoadTenantByNameAsync(tenantName);
			if (tenant == null)
			{
				return NotFound();
			}

			return Ok(tenant.ToModel());
		}

		/// <summary>
		/// Creates a new tenant.
		/// </summary>
		/// <param name="tenant">The tenant to create.</param>
		/// <returns></returns>
		[HttpPost]
		[SwaggerResponse(201, "Response with the uri of the newly created tenant.")]
		[SwaggerResponse(409, "Tenant could not be created because a tenant with the same name or id already existed.")]
		[SwaggerResponse(422, "Tenant could not be created because the provided data was invalid.")]
		public async Task<IActionResult> CreateTenant([FromBody] Tenant? tenant)
		{
			if (tenant == null) return UnprocessableEntity();

			try
			{
				var id = await _tenantRepository.CreateTenantAsync(tenant.ToTenant());
				return CreatedAtAction(nameof(GetTenantById), new { TenantId = id }, id);
			}
			catch
			{
				return Conflict();
			}
		}

		/// <summary>
		/// Creates a client secret for the <see cref="Tenant"/> to authenticate with.
		/// </summary>
		/// <param name="tenantId">The id of the <see cref="Tenant"/> to set a client secret for.</param>
		/// <param name="secret">An optional secret. If none is provided, a random secret will be generated and returned.</param>
		/// <returns>The secret the <see cref="Tenant"/> can use to authenticate against the security token service.</returns>
		[HttpPost("{tenantId:guid}/secret")]
		[SwaggerResponse(200, "Secret for the Tenant with given id.", typeof(Secret))]
		[SwaggerResponse(404, "Could not create secret because tenant with given id was not found.", typeof(Secret))]
		public async Task<ActionResult<Secret>> CreateClientSecret([FromRoute] Guid tenantId, string? secret = null)
		{
			var tenant = await _tenantRepository.LoadTenantByIdAsync(tenantId);
			if (tenant == null) return NotFound();

			secret ??= KeyGenerator.GetUniqueKey(8);
			var hash = secret.Sha512();

			await _tenantRepository.CreateClientSecretAsync(new ClientSecret()
			{
				Id = Guid.NewGuid(),
				Created = DateTime.UtcNow,
				TenantId = tenantId,
				Value = hash
			});

			return Ok(new Secret(secret));
		}

		/// <summary>
		/// Deletes a tenant with a given id.
		/// </summary>
		/// <param name="tenantId">The id of the <see cref="Tenant"/> to delete.</param>
		/// <returns>200, if deletion was successful; 404 if tenant was not found.</returns>
		[HttpDelete("{tenantId:guid}")]
		[SwaggerResponse(200, "Tenant with given id was successfully deleted.")]
		[SwaggerResponse(404, "Tenant with given id was not found.")]
		public async Task<IActionResult> DeleteTenantById([FromRoute] Guid tenantId)
		{
			if (await _tenantRepository.DeleteTenantByIdAsync(tenantId))
			{
				return Ok();
			}

			return NotFound();
		}

		// TODO: Re-do that properly
		// ReSharper disable once ClassNeverInstantiated.Local
		private class KeyGenerator
		{
			private static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

			public static string GetUniqueKey(int size)
			{
				var data = new byte[4 * size];

				using var crypto = new RNGCryptoServiceProvider();
				crypto.GetBytes(data);

				var result = new StringBuilder(size);
				for (var i = 0; i < size; i++)
				{
					var rnd = BitConverter.ToUInt32(data, i * 4);
					var idx = rnd % Chars.Length;

					result.Append(Chars[idx]);
				}

				return result.ToString();
			}
		}
	}

	internal static class HashExtensions
	{
		/// <summary>
		/// Creates a SHA512 hash of the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>A hash</returns>
		public static string Sha512(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;

			using var sha = SHA512.Create();
			var bytes = Encoding.UTF8.GetBytes(input);
			var hash = sha.ComputeHash(bytes);

			return Convert.ToBase64String(hash);
		}
	}
}
