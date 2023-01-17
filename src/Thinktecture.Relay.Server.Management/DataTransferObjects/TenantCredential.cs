using System;

namespace Thinktecture.Relay.Server.Management.DataTransferObjects;

/// <summary>
/// Represents a stored credential for a tenant.
/// </summary>
public class TenantCredential
{
	/// <summary>
	/// Gets or sets a unique identifier for this specific credential.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Gets or sets a SHA256 or SHA512 value that represents the value of this credential.
	/// </summary>
	public string? Value { get; set; }

	/// <summary>
	/// Gets or sets a plain text value. To be used only for creating a new credential.
	/// </summary>
	public string? PlainTextValue { get; set; }

	/// <summary>
	/// Gets or sets the date and time when this credential was created at.
	/// </summary>
	public DateTime Created { get; set; }

	/// <summary>
	/// Gets or sets the date and time when this credential expires.
	/// </summary>
	public DateTime? Expiration { get; set; }
}
