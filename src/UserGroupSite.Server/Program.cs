using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Debugging;
using UserGroupSite.Data.Models;
using UserGroupSite.Server.Components;
using _Imports = UserGroupSite.Client._Imports;

SelfLog.Enable(msg => Debug.WriteLine(msg));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

var isMigrations = Environment.GetCommandLineArgs()[0].Contains("ef.dll");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // put things in here that only need to run when we aren't running migrations
    // good example of that is that we don't need to configure serilog when we aren't actually running our app
    if (!isMigrations)
    {
        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration));
    }

    // Add "core" services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    // End "core" services to the container
    
    // Add application stuff
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            .EnableSensitiveDataLogging());
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("SiteOptions"));
    // End application stuff

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(_Imports).Assembly);

    if (!isMigrations)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // InitializeData.Initialize(serviceProvider);
    }

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException" &&
                           ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled Exception.");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}