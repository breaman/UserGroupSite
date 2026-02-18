using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserGroupSite.Client.Services;
using UserGroupSite.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });

// Register application services
builder.Services.AddScoped<ISpeakerService, SpeakerService>();

await builder.Build().RunAsync();