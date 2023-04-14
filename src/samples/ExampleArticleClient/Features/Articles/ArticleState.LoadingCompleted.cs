using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record LoadingCompletedAction;

public partial class ArticleStateReducers
{
	[ReducerMethod]
	public static ArticleState Reduce(ArticleState state, LoadingCompletedAction _) =>
		state with
		{
			IsLoading = false,
		};
}
