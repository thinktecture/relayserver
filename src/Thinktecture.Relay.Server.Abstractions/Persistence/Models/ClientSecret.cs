using System;

namespace Thinktecture.Relay.Server.Persistence.Models;

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
	/// Indicates the point in time when this secret was created.
	/// </summary>
	public DateTime Created { get; set; }

	/// <summary>
	/// Defines an optional point in time when this secret automatically will become invalid.
	/// </summary>
	public DateTime? Expiration { get; set; }

	/// <summary>
	/// Update this instance with all values from the other instance.
	/// </summary>
	/// <param name="other">The source to copy the data over from to this instance.</param>
	public void UpdateFrom(ClientSecret other)
	{
		if (Id != other.Id)
		{
			throw new InvalidOperationException(
				$"Id of other secret: {other.Id} is not the same as this: {Id}. Cannot update from other instance.");
		}

		Created = other.Created;
		Value = other.Value;
		Expiration = other.Expiration;
	}
}
