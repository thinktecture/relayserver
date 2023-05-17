using System.Net.Http.Json;
using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record Header(string Name, string Value);
public record LoadAction(string? Tenant = null, string? AdditionalParameters = null, Header[]? Headers = null);

public partial class ArticleStateReducers
{
	[ReducerMethod]
	public static ArticleState Reduce(ArticleState state, LoadAction _) =>
		state with
		{
			IsLoading = true,
			ErrorMessage = null,
			Articles = Array.Empty<Article>(),
			Requests = Array.Empty<RequestInfo>(),
		};
}

public partial class ArticleStateEffects
{
	private CancellationTokenSource? _cancellationTokenSource;

	[EffectMethod]
	public async Task HandleLoadAction(LoadAction action, IDispatcher dispatcher)
	{
		_cancellationTokenSource?.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();

		try
		{
			var token = _cancellationTokenSource.Token;

			var client = GetHttpClient();
			client.BaseAddress = new Uri(String.IsNullOrEmpty(action.Tenant)
				? SettingsState.Value.UrlForLocalApi()
				: SettingsState.Value.UrlForRelayServer(action.Tenant));

			// add additional parameters and headers
			var articlesRequest = "articles";
			if (!String.IsNullOrEmpty(action.AdditionalParameters))
				articlesRequest += action.AdditionalParameters;

			if (action.Headers is not null)
			{
				foreach (var header in action.Headers)
				{
					client.DefaultRequestHeaders.Add(header.Name, header.Value);
				}
			}

			// Do this sequentially to be able to see this request in the requests list
			var articles = await client.GetFromJsonAsync<Article[]>(articlesRequest, token);
			dispatcher.Dispatch(new ArticlesLoadedAction(articles));

			// Re-create client. We don't want to send the headers from the previous request
			client = GetHttpClient();
			client.BaseAddress = new Uri(String.IsNullOrEmpty(action.Tenant)
				? SettingsState.Value.UrlForLocalApi()
				: SettingsState.Value.UrlForRelayServer(action.Tenant));

			var requests = await client.GetFromJsonAsync<RequestInfo[]>("requests", token);
			dispatcher.Dispatch(new RequestsLoadedAction(requests));

			dispatcher.Dispatch(new LoadingCompletedAction());
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			dispatcher.Dispatch(new LoadingFailedAction(ex.Message));
		}
		finally
		{
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}
	}
}
