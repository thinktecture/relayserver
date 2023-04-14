using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record LoadingFailedAction(string ErrorMessage);

public partial class ArticleStateReducers
{
	[ReducerMethod]
	public static ArticleState Reduce(ArticleState state, LoadingFailedAction action) =>
		state with
		{
			IsLoading = false,
			ErrorMessage = action.ErrorMessage,
			Articles = Array.Empty<Article>(),
			Requests = Array.Empty<RequestInfo>(),
		};
}
