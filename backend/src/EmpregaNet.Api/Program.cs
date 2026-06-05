using EmpregaNet.Api.Middleware;
using EmpregaNet.AI;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Extensions;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Seeds;

var builder = WebApplication.CreateBuilder(args);

var builderServices = builder.Services;
builderServices.AddMediator();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

// Register dependencies
builderServices.RegisterApiDependencies();
builderServices.RegisterApplicationDependencies();
builderServices.RegisterAIDependencies(builder.Configuration);
builder.RegisterCoreDependencies();

var app = builder.Build();

// Aplica migrações pendentes em qualquer ambiente - DEV/HML/PROD
app.ApplyPendingMigrations();

await IdentityDataSeeder.SeedAsync(app);

#region Configure Pipeline

app.SetupApiServices();
app.UseExceptionHandler();
app.UseSentryTracingMiddleware();
app.UseRateLimiter();
app.Run();

#endregion



