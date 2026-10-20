using EmpregaNet.Api.Middleware;
using EmpregaNet.AI;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Startup;

var builder = WebApplication.CreateBuilder(args);

var builderServices = builder.Services;
builderServices.AddMediator("EmpregaNet");
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

// Register dependencies
builderServices.RegisterApiDependencies(builder.Configuration, builder.Environment);
builderServices.RegisterApplicationDependencies();
builderServices.RegisterAIDependencies(builder.Configuration);
builder.RegisterCoreDependencies();

var app = builder.Build();

// Migrações, seed e key ring sob bloqueio entre réplicas; com --migrate-only roda só isto e encerra.
var migrateOnly = args.Contains(StartupExtensions.MigrateOnlyArgument);
await app.RunStartupTasksAsync(migrateOnly);

if (migrateOnly)
{
    return;
}

#region Configure Pipeline

app.SetupApiServices();
app.UseExceptionHandler();
app.UseSentryTracingMiddleware();
app.Run();

#endregion



