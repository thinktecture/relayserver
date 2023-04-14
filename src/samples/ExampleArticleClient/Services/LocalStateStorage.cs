using Blazored.LocalStorage;
using Fluxor.Persist.Storage;

namespace ExampleArticleClient.Services;

public class LocalStateStorage : IStringStateStorage
{
	private readonly ILocalStorageService _localStorageService;

	public LocalStateStorage(ILocalStorageService localStorageService)
	{
		_localStorageService = localStorageService;
	}

	public async ValueTask<string> GetStateJsonAsync(string statename)
	{
		return await _localStorageService.GetItemAsStringAsync(statename);
	}

	public async ValueTask StoreStateJsonAsync(string statename, string json)
	{
		await _localStorageService.SetItemAsStringAsync(statename, json);
	}
}
