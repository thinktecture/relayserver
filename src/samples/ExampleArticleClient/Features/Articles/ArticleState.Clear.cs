using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record ClearAction;

public partial class ArticleStateReducers
{
	[ReducerMethod]
	public static ArticleState Reduce(ArticleState state, ClearAction _) =>
		state with
		{
			IsLoading = false,
			ErrorMessage = null,
			Articles = Array.Empty<Article>(),
			Requests = Array.Empty<RequestInfo>(),
		};
}
