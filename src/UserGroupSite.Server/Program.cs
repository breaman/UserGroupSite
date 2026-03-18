using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;
using UserGroupSite.Server.Components;
using UserGroupSite.Server.Components.Account;
using UserGroupSite.Server.Components.Email;
using UserGroupSite.Server.Endpoints;
using UserGroupSite.Server.Services;
using UserGroupSite.ServiceDefaults;
using UserGroupSite.Data.Interfaces;
using UserGroupSite.Shared.Services;
using UserGroupSite.Server.Models;
using Microsoft.Extensions.Options;
using Mailjet.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddHttpContextAccessor();

// Register application services
builder.Services.AddScoped<ISpeakerService, SpeakerService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITopicSuggestionService, TopicSuggestionService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, HttpUserService>();
builder.Services.AddSingleton<IMarkdownService, MarkdownService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMeetupService, MeetupService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(Constants.DatabaseConnectionString))
        .EnableSensitiveDataLogging());
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(Constants.DatabaseConnectionString))
        .EnableSensitiveDataLogging(), ServiceLifetime.Scoped);
builder.EnrichSqlServerDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedAccount = true;

        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    // AddRoles isn't added from the AddIdentityCore, so if you want to use roles, this must be explicitly added
    .AddRoles<Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddKeyedScoped<IEmailSender<User>, IdentityNoOpEmailSender>("noop");
builder.Services.AddKeyedScoped<IEmailSender<User>, IdentityMailjetEmailSender>("mailjet");
builder.Services.AddScoped(serviceProvider =>
    {
        var options =serviceProvider.GetRequiredService<IOptions<AppSettings>>();
        return serviceProvider.GetRequiredKeyedService<IEmailSender<User>>(options.Value.Email?.ServiceName);
    });

builder.Services.AddHttpClient<IMailjetClient, MailjetClient>(client =>
{
    client.SetDefaultSettings();
    client.UseBasicAuthentication(builder.Configuration.GetValue<string>("MailJet:ApiKey"),
        builder.Configuration.GetValue<string>("MailJet:ApiSecret"));
});

// Add route configuration to enforce lowercase URLs for better SEO
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
    options.AppendTrailingSlash = false;
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(UserGroupSite.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();
app.MapEventEndpoints();
app.MapCommentEndpoints();
app.MapTopicSuggestionEndpoints();
app.MapUserEndpoints();

app.Run();
