using System.Security.Claims;
using EmpregaNet.Api.Configurations;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Common.Behaviors;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Services;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApplication();
builder.AddApiConfiguration();

var builderServices = builder.Services;

builderServices.AddMediator(typeof(Program).Assembly);
builderServices.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builderServices.ConfigureServices(builder.Configuration);
builderServices.ConfigureCorsPolicy();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

builderServices.AddHealthChecks()
               .AddCheck<DatabaseCheck>("Database")
               .AddCheck<MemoryServiceCheck>("Cache");

var app = builder.Build();

#region Configure Pipeline

app.UseApiConfiguration(app.Environment);

app.MapGet("/whoAmI", async (ClaimsPrincipal claims, PostgreSqlContext context) =>
{
    var userId = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await context.Users.FindAsync(userId);
    return user;
}).RequireAuthorization();

app.UseExceptionHandler();

app.Run();

#endregion



