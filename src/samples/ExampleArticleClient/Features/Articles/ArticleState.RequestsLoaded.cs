using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record RequestsLoadedAction(RequestInfo[] RequestInfo);

public partial class ArticleStateReducers
{
	[ReducerMethod]
	public static ArticleState Reduce(ArticleState state, RequestsLoadedAction action) =>
		state with
		{
			Requests = action.RequestInfo,
		};
}
