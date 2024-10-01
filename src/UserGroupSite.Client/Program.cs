using Blazored.Toast;
using FluentValidation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserGroupSite.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddBlazoredToast();
builder.Services.AddTransient<IValidator<EventDto>, EventDtoValidator>();

builder.Services.AddScoped<ICategoryService, ClientCategoryService>();
builder.Services.AddScoped<IEventService, ClientEventService>();
builder.Services.AddScoped<IApplicationUserService, ClientApplicationUserService>();

await builder.Build().RunAsync();