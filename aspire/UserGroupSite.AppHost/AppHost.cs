using System.Diagnostics;
using UserGroupSite.AppHost;
using UserGroupSite.ServiceDefaults;

var osArch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;

var builder = DistributedApplication.CreateBuilder(args);

var dbPassword = builder.AddParameter("sql-password", "P@ssw0rd!")
    .InitiallyHidden();

var sqlServer = builder.AddSqlServer("sqlserver", dbPassword)
    .WithContainerName("usergroupsite-sqlserver");

if (osArch == System.Runtime.InteropServices.Architecture.Arm64
    && System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
{
    sqlServer.WithImage("azure-sql-edge");
}

var db = sqlServer.WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase(Constants.DatabaseConnectionString);

//DBGate is a database viewer
var dbGate = builder.AddContainer("dbgate", "dbgate/dbgate")
    .WithExplicitStart()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerName("case-amp-db-gate")
    .WithHttpEndpoint(targetPort: 3000)
    .WaitFor(sqlServer)
    .WithEnvironment("CONNECTIONS", "mssql")
    .WithEnvironment("LABEL_mssql", "MS SQL")
    .WithEnvironment("SERVER_mssql", "host.docker.internal")
    .WithEnvironment("PORT_mssql", () => (
        $"{sqlServer.Resource.PrimaryEndpoint.Port}"
    ))
    .WithEnvironment("USER_mssql", "sa")
    .WithEnvironment("PASSWORD_mssql", dbPassword)
    .WithEnvironment("ENGINE_mssql", "mssql@dbgate-plugin-mssql")
    .WithParentRelationship(sqlServer);

var migrations = builder.AddExecutable("db-migrations", "dotnet", "../../src/UserGroupSite.Server",
    [
        "ef",
        "database",
        "update",
        "--no-build",
        "--project",
        "../UserGroupSite.Data",
        "--connection",
        db.Resource.ConnectionStringExpression
    ])
    .WithCommand("dotnet-tools", "Restore Tools", async (ExecuteCommandContext x) =>
    {
        var process = Process.Start(new ProcessStartInfo()
        {
            FileName = "dotnet",
            ArgumentList = { "tool", "restore" },
        });
        if (process is null) return CommandResults.Failure();
        await process.WaitForExitAsync(x.CancellationToken);
        return CommandResults.Success();
    }, new CommandOptions())
    //.HideWhen(KnownResourceStates.Finished)
    .WaitFor(db);

builder.AddProject<Projects.UserGroupSite_Server>("server", "https")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck(Constants.HealthEndpointPath)
    .WithReference(db)
    .WaitForCompletion(migrations);

builder.Build().Run();
