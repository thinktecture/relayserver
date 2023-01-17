namespace Thinktecture.Relay.Server.Management;

/// <summary>
///  Provides constant values for the management api defaults.
/// </summary>
public static class ManagementApiPolicyNames
{

	/// <summary>
	/// The default policy name for the read permissions.
	/// </summary>
	public const string Read = "managementapi:read";

	/// <summary>
	/// The default policy name for the write permissions.
	/// </summary>
	public const string Write = "managementapi:write";
}
