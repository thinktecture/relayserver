using Fluxor;
using Fluxor.Persist.Storage;

namespace ExampleArticleClient.Features.Settings;

[FeatureState, PersistState]
public record SettingsState
{
	public string LocalApiBaseUri { get; init; } = "https://localhost:7100";
	public string RelayServerBaseUri { get; init; } = "https://localhost:5001";
	public string ApiBasePath { get; init; } = "/api";
	public string RelayServerPath { get; init; } = "/relay/{Tenant}/{Target}/{ApiBasePath}";
	public string Target { get; init; } = "ExampleArticleApi";
	public string[] Tenants { get; init; } = new[] { "TestTenant1", "TestTenant2", };

	public string UrlForLocalApi() => LocalApiBaseUri + ApiBasePath + "/";

	public string UrlForRelayServer(string tenant)
		=> RelayServerBaseUri +
			RelayServerPath
				.Replace("{Tenant}", tenant)
				.Replace("{Target}", Target)
				.Replace("{ApiBasePath}", ApiBasePath.StartsWith("/")
					? ApiBasePath[1..]
					: ApiBasePath)
			+ "/";

	public SettingsModel ToModel()
	{
		return new()
		{
			LocalApiBaseUri = LocalApiBaseUri,
			RelayServerBaseUri = RelayServerBaseUri,
			ApiBasePath = ApiBasePath,
			RelayServerPath = RelayServerPath,
			Target = Target,
			Tenants = Tenants.ToList(),
		};
	}
}

public record SaveStateAction(SettingsState NewState);

public class SettingsReducers
{
	[ReducerMethod]
	public static SettingsState ReduceSaveStateAction(SettingsState _, SaveStateAction action) => action.NewState;
}
