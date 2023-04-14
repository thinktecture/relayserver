using Fluxor;

namespace ExampleArticleClient.Features.Articles;

public record CancelLoadAction;

public partial class ArticleStateEffects
{
	[EffectMethod]
	public Task HandleCancelLoadAction(CancelLoadAction _, IDispatcher dispatcher)
	{
		_cancellationTokenSource?.Cancel();
		dispatcher.Dispatch(new ClearAction());

		return Task.CompletedTask;
	}
}
