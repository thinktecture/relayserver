using Microsoft.AspNetCore.Components;
using Fluxor;
using ExampleArticleClient.Features.Settings;

namespace ExampleArticleClient.Features.Articles;

public partial class ArticlesApiClient
{
	[Inject] protected IState<SettingsState> SettingsState { get; set; }
	[Inject] protected IDispatcher Dispatcher { get; set; }
	[Inject] protected IState<ArticleState> ArticleState { get; set; }

	private ArticleState State => ArticleState.Value;
	private SettingsState Settings => SettingsState.Value;

	private bool UseRelayServer;
	private string SelectedTenant = String.Empty;
	private string UrlToRequest =>
		(UseRelayServer
			? Settings.UrlForRelayServer(SelectedTenant)
			: Settings.UrlForLocalApi())
		+ AdditionalParameters;

	private string AdditionalParameters = String.Empty;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		SelectedTenant = Settings.Tenants.FirstOrDefault() ?? String.Empty;
	}

	private void Load()
	{
		Dispatcher.Dispatch(new LoadAction(UseRelayServer ? SelectedTenant : null, AdditionalParameters));
	}

	private void Cancel()
	{
		Dispatcher.Dispatch(new CancelLoadAction());
	}
}
