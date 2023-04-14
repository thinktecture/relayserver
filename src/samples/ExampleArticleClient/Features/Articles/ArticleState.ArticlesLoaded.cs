using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record ArticlesLoadedAction(Article[] Articles);

public partial class ArticleStateReducers
{
	[ReducerMethod]
	public static ArticleState Reduce(ArticleState state, ArticlesLoadedAction action) =>
		state with
		{
			Articles = action.Articles,
		};
}
