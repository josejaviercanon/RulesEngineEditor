using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RulesEngine.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
{
    var backendUrl = builder.Configuration["BackendUrl"] ?? "https://localhost:7086";
    return new HttpClient { BaseAddress = new Uri(backendUrl) };
});

await builder.Build().RunAsync();
