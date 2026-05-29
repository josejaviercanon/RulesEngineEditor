using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RulesEngine.UI;
using RulesEngine.UI.Services;
using Blazorise;
using Blazorise.Tailwind;
using Blazorise.Icons.Lucide;
using Blazorise.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
{
    var backendUrl = builder.Configuration["BackendUrl"] ?? "https://localhost:7086";
    return new HttpClient { BaseAddress = new Uri(backendUrl) };
});

builder.Services.AddScoped<WorkflowApiClient>();
builder.Services.AddScoped<WorkflowManagementService>();

builder.Services
    .AddBlazorise()
    .AddTailwindProviders()
    .AddLucideIcons()
    .AddTailwindComponents()
    .AddBlazoriseRouterTabs();
    

await builder.Build().RunAsync();
