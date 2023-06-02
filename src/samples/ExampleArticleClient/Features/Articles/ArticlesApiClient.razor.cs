using Microsoft.AspNetCore.Components;
using Fluxor;
using ExampleArticleClient.Features.Settings;
using System.Text;

namespace ExampleArticleClient.Features.Articles;

public partial class ArticlesApiClient
{
	[Inject] protected IState<SettingsState> SettingsState { get; set; }
	[Inject] protected IDispatcher Dispatcher { get; set; }
	[Inject] protected IState<ArticleState> ArticleState { get; set; }

	private bool requestExpanded = true;
	private bool responseExpanded = true;

	private ArticleState State => ArticleState.Value;
	private SettingsState Settings => SettingsState.Value;

	private bool UseRelayServer;
	private string SelectedTenant = String.Empty;
	private string UrlToRequest =>
		(UseRelayServer
			? Settings.UrlForRelayServer(SelectedTenant)
			: Settings.UrlForLocalApi())
		+ AdditionalParameters;

	private bool SendHeader = false;
	private string HeaderName = "tt-relay-metadata";
	private string HeaderValue;
	private string AdditionalParameters = String.Empty;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		SelectedTenant = Settings.Tenants.FirstOrDefault() ?? String.Empty;
		HeaderValue = GetNewHeaderValue();
	}

	private void Load()
	{
		Dispatcher.Dispatch(new LoadAction(UseRelayServer ? SelectedTenant : null, AdditionalParameters, BuildHeaders()));
		HeaderValue = GetNewHeaderValue();
	}

	private void Cancel()
	{
		Dispatcher.Dispatch(new CancelLoadAction());
	}

	private Header[]? BuildHeaders()
	{
		if (!SendHeader)
			return null;

		var result = new[]
		{
			new Header(HeaderName, Convert.ToBase64String(Encoding.UTF8.GetBytes(HeaderValue))),
		};

		return result;
	}

	private string GetNewHeaderValue()
	{
		return $$"""
		{
			"traceId": "{{Guid.NewGuid()}}",
			"client": "Article - API - Client",
			"timestamp": "{{DateTimeOffset.Now:O}}",

			"sourceTenant": "Tenant 1",
			"sourceApplication": "App1",
			"sourceFolder": "C:\\app1\\outgoing\\",
			"sourceMessageType": "4711",

			"files": [
				{ "fileName": "some-data.json", "fileSize": 104304 },
				{ "fileName": "some-metadata.json", "fileSize": 350 }
			]
		}
		""";
	}
}
