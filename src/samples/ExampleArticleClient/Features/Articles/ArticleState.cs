using System.Text.Json.Serialization;
using Fluxor;
using ExampleArticleClient.Services;
using ExampleArticleClient.Features.Settings;

namespace ExampleArticleClient.Features.Articles;

public record Article(string Title, Uri Url, string[] Tags, string[] Authors, [property: JsonConverter(typeof(DateOnlyConverter))] DateOnly Date, string Language);
public record RequestInfo(DateTimeOffset Time, string Method, string Path, string Query, string[] Headers, string IpAddress);

[FeatureState]
public record ArticleState
{
	public bool IsLoading { get; init; } = false;
	public Article[] Articles { get; init; } = Array.Empty<Article>();
	public RequestInfo[] Requests { get; init; } = Array.Empty<RequestInfo>();
	public string? ErrorMessage { get; init; } = null;

	public bool HasData => Articles.Any();
}

public partial class ArticleStateReducers { }

public partial class ArticleStateEffects
{
	protected readonly IState<SettingsState> SettingsState;

	public ArticleStateEffects(IState<SettingsState> settingsState)
	{
		SettingsState = settingsState;
	}

	protected HttpClient GetHttpClient()
	{
		var client = new HttpClient();

		client.DefaultRequestHeaders.Add("Accept", "application/json");
		client.DefaultRequestHeaders.Add("User-Agent", "Blazor");

		return client;
	}
}
