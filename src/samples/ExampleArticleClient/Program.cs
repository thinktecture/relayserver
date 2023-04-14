using Blazored.LocalStorage;
using ExampleArticleClient;
using ExampleArticleClient.Services;
using Fluxor;
using Fluxor.Persist.Middleware;
using Fluxor.Persist.Storage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<IStringStateStorage, LocalStateStorage>();
builder.Services.AddScoped<IStoreHandler, JsonStoreHandler>();
builder.Services.AddFluxor(o =>
{
	o.ScanAssemblies(typeof(Program).Assembly);
	o.UsePersist();

	o.UseReduxDevTools();
});



await builder.Build().RunAsync();
