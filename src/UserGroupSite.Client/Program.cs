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
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITopicSuggestionService, TopicSuggestionService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddSingleton<IMarkdownService, MarkdownService>();

await builder.Build().RunAsync();