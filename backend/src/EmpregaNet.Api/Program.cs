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

// Espera o banco, aplica (ou exige) as migrações, faz o seed e garante o key ring do Data Protection, tudo sob um
// bloqueio entre réplicas. O host só começa a escutar depois disto, então nenhuma réplica recebe tráfego antes de
// estar apta. Com --migrate-only roda só isto e encerra (job de migração, separado do boot das réplicas).
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



