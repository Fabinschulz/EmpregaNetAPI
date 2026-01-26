using EmpregaNet.Api.Middleware;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);

var builderServices = builder.Services;
builderServices.AddMediator();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

// Register dependencies
builderServices.RegisterApiDependencies();
builderServices.RegisterApplicationDependencies();
builder.RegisterCoreDependencies();

var app = builder.Build();

if (app.Environment.IsProduction()) 
{
    // Executa migrações apenas em produção/staging automaticamente
    app.ApplyPendingMigrations();
}

#region Configure Pipeline

app.SetupApiServices();
app.UseExceptionHandler();
app.UseSentryTracingMiddleware();
app.UseRateLimiter();
app.Run();

#endregion



