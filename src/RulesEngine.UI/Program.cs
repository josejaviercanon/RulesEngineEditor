using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RulesEngine.UI;
using Blazorise;
using Blazorise.Tailwind;
using Blazorise.Icons.Lucide;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
{
    var backendUrl = builder.Configuration["BackendUrl"] ?? "https://localhost:7086";
    return new HttpClient { BaseAddress = new Uri(backendUrl) };
});

builder.Services
    .AddBlazorise()
    .AddTailwindProviders()
    .AddLucideIcons();

await builder.Build().RunAsync();
