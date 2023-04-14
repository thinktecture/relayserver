using Fluxor;
using Microsoft.AspNetCore.Components;

namespace ExampleArticleClient.Features.Settings;

public partial class SettingsForm
{
	[Inject] protected IState<SettingsState> SettingsState { get; set; }
	[Inject] protected IDispatcher Dispatcher { get; set; }
	private SettingsState State => SettingsState.Value;

	protected SettingsModel FormModel { get; set; } = new();

	protected override void OnInitialized()
	{
		base.OnInitialized();

		FormModel = State.ToModel();
	}

	protected void Save()
	{
		Dispatcher.Dispatch(new SaveStateAction(FormModel.ToState()));
	}

	protected void Reset()
	{
		FormModel = State.ToModel();
	}
}
