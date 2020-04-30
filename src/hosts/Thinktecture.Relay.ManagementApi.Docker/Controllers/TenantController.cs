using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.ManagementApi.Docker.Controllers
{
	[AllowAnonymous]
	[Route("{controller}")]
	public class TenantController : Controller
	{
		private readonly ITenantRepository _tenantRepository;

		public TenantController(ITenantRepository tenantRepository)
		{
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
		}

		[HttpGet]
		public IAsyncEnumerable<Tenant> GetAllTenants(int skip = 0, int take = 10)
		{
			return _tenantRepository.LoadAllTenantsPagedAsync(skip, take);
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<Tenant>> GetTenantById([FromRoute] Guid id)
		{
			var tenant = await _tenantRepository.LoadTenantByIdAsync(id);
			tenant?.ClientSecrets.Clear();
			return Ok(tenant);
		}

		[HttpGet("{name:alpha}")]
		public async Task<ActionResult<Tenant>> GetTenantByName([FromRoute] string name)
		{
			var tenant = await _tenantRepository.LoadTenantByNameAsync(name);
			tenant?.ClientSecrets.Clear();
			return Ok(tenant);
		}

		[HttpPost]
		public async Task<IActionResult> CreateTenant([FromBody] Tenant tenantToCreate)
		{
			try
			{
				var id = await _tenantRepository.CreateTenantAsync(tenantToCreate);
				return CreatedAtAction(nameof(GetTenantById), new { id }, new { id });
			}
			catch
			{
				return Conflict();
			}
		}

		[HttpPost("{tenantId:guid}/secret")]
		public async Task<IActionResult> CreateClientSecret([FromRoute] Guid tenantId)
		{
			var secret = KeyGenerator.GetUniqueKey(8);
			var hash = secret.Sha512();

			await _tenantRepository.CreateClientSecretAsync(new ClientSecret()
			{
				Id = Guid.NewGuid(),
				Created = DateTime.UtcNow,
				TenantId = tenantId,
				Value = hash,
			});

			return Ok(new { secret });
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteTenantById([FromRoute] Guid id)
		{
			if (await _tenantRepository.DeleteTenantByIdAsync(id))
				return Ok();

			return NotFound();
		}

		// TODO: Re-do that properly
		private class KeyGenerator
		{
			internal static readonly char[] chars =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray(); 

			public static string GetUniqueKey(int size)
			{
				byte[] data = new byte[4*size];
				using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
				{
					crypto.GetBytes(data);
				}

				StringBuilder result = new StringBuilder(size);
				for (int i = 0; i < size; i++)
				{
					var rnd = BitConverter.ToUInt32(data, i * 4);
					var idx = rnd % chars.Length;

					result.Append(chars[idx]);
				}

				return result.ToString();
			}

			public static string GetUniqueKeyOriginal_BIASED(int size)
			{
				char[] chars =
					"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
				byte[] data = new byte[size];
				using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
				{
					crypto.GetBytes(data);
				}
				StringBuilder result = new StringBuilder(size);
				foreach (byte b in data)
				{
					result.Append(chars[b % (chars.Length)]);
				}
				return result.ToString();
			}
		}
	}

	public static class HashExtensions
	{
		/// <summary>
		/// Creates a SHA256 hash of the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>A hash</returns>
		public static string Sha256(this string input)
		{
			if (String.IsNullOrEmpty(input)) return String.Empty;

			using (var sha = SHA256.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(input);
				var hash = sha.ComputeHash(bytes);

				return Convert.ToBase64String(hash);
			}
		}

		/// <summary>
		/// Creates a SHA256 hash of the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>A hash.</returns>
		public static byte[] Sha256(this byte[] input)
		{
			if (input == null)
			{
				return null;
			}

			using (var sha = SHA256.Create())
			{
				return sha.ComputeHash(input);
			}
		}

		/// <summary>
		/// Creates a SHA512 hash of the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>A hash</returns>
		public static string Sha512(this string input)
		{
			if (String.IsNullOrEmpty(input)) return String.Empty;

			using (var sha = SHA512.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(input);
				var hash = sha.ComputeHash(bytes);

				return Convert.ToBase64String(hash);
			}
		}
	}
}
