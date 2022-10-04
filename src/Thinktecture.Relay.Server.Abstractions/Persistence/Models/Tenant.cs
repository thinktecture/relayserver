using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Server.Persistence.Models;

/// <summary>
/// Represents a tenant. A tenant can have multiple connectors on one physical on-premises.
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
	/// <remarks>The maximum length is 100 unicode characters.</remarks>
	public string Name { get; set; } = default!;

	/// <summary>
	/// The display name of the tenant. Will be used as a visual identifier on the management UI.
	/// </summary>
	/// <remarks>The maximum length is 200 unicode characters.</remarks>
	public string? DisplayName { get; set; }

	/// <summary>
	/// An optional, longer, textual description of this tenant.
	/// </summary>
	/// <remarks>The maximum length is 1000 unicode characters.</remarks>
	public string? Description { get; set; }

	/// <summary>
	/// The normalized (e.g. ToUpperInvariant()) name of the tenant. Use this for case-insensitive comparison in the database.
	/// </summary>
	/// <remarks>The maximum length is 100 unicode characters.</remarks>
	[JsonIgnore]
	public string NormalizedName { get; set; } = default!;

	/// <summary>
	/// The client secrets, used for authentication connectors for this tenant.
	/// </summary>
	[JsonIgnore]
	public List<ClientSecret>? ClientSecrets { get; set; }

	/// <summary>
	/// The connections for this tenant.
	/// </summary>
	[JsonIgnore]
	public List<Connection>? Connections { get; set; }

	/// <summary>
	/// The requests handled for this tenant.
	/// </summary>
	[JsonIgnore]
	public List<Request>? Requests { get; set; }

	/// <summary>
	/// The config for this tenant.
	/// </summary>
	[JsonIgnore]
	public Config? Config { get; set; }

	/// <summary>
	/// If both tenants have the same id, update this instance with all values from the other instance.
	/// </summary>
	/// <param name="other">The source to copy the data over from to this instance.</param>
	public void UpdateFrom(Tenant other)
	{
		if (Id != other.Id)
		{
			throw new InvalidOperationException(
				$"Id of other tenant: {other.Id} is not the same as this: {Id}. Cannot update from other instance.");
		}

		Name = other.Name;
		NormalizedName = other.NormalizedName;

		DisplayName = other.DisplayName;
		Description = other.Description;

		// Copy over config if available
		if (other.Config != null)
		{
			Config ??= new Config();
			Config.UpdateFrom(other.Config);
		}

		// Copy over secrets when they are complete with values
		if (other.ClientSecrets != null)
		{
			ClientSecrets ??= new List<ClientSecret>();
			ClientSecrets.RemoveAll(cs => !other.ClientSecrets.Any(os => os.Id == cs.Id));

			// only consider secrets that actually have a value
			foreach (var secret in other.ClientSecrets.Where(o => !String.IsNullOrWhiteSpace(o.Value)))
			{
				var existingSecret = ClientSecrets.SingleOrDefault(cs => cs.Id == secret.Id);
				if (existingSecret == null)
				{
					// create new secret, either with given id or with a new one
					if (secret.Id == Guid.Empty)
					{
						secret.Id = Guid.NewGuid();
					}

					secret.Created = DateTime.UtcNow;

					ClientSecrets.Add(existingSecret = new ClientSecret() { Id = secret.Id, });
				}

				existingSecret.UpdateFrom(secret);
			}
		}
	}
}
