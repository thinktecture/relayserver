namespace ExampleArticleClient.Features.Settings;

public class SettingsModel
{
	public string LocalApiBaseUri { get; set; }
	public string RelayServerBaseUri { get; set; }
	public string ApiBasePath { get; set; }
	public string RelayServerPath { get; set; }
	public string Target { get; set; }
	public List<string> Tenants { get; set; }

	public SettingsState ToState()
	{
		return new()
		{
			LocalApiBaseUri = LocalApiBaseUri,
			RelayServerBaseUri = RelayServerBaseUri,
			ApiBasePath = ApiBasePath,
			RelayServerPath = RelayServerPath,
			Target = Target,
			Tenants = Tenants.ToArray(),
		};
	}
}
