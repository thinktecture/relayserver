namespace Thinktecture.Relay.Server.Management;

/// <summary>
///  Provides constant values for the management api defaults.
/// </summary>
public static class ManagementApiConstants
{
	/// <summary>
	/// The default path to the management api.
	/// </summary>
	public const string DefaultBasePath = "/api/management";

	/// <summary>
	/// The default path to the tenants endpoint.
	/// </summary>
	public const string DefaultTenantsPath = "/tenants";

	/// <summary>
	/// The default policy name for the read permissions.
	/// </summary>
	public const string DefaultReadPolicyName = "managementapi:read";

	/// <summary>
	/// The default policy name for the write permissions.
	/// </summary>
	public const string DefaultWritePolicyName = "managementapi:write";
}
