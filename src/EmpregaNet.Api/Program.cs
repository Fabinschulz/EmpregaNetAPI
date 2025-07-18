using System.Security.Claims;
using EmpregaNet.Api.Configurations;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Service;
using EmpregaNet.Domain.Components.Mediator.Extensions;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Configurations;
using EmpregaNet.Infra.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterInfraDependency();
builder.AddApiConfiguration();

var builderServices = builder.Services;
builderServices.AddMediator();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();
builderServices.RegisterApplicationDependency();

var app = builder.Build();

#region Configure Pipeline

app.UseApiConfiguration(app.Environment);
app.UseSentryTracingMiddleware();

app.MapGet("/whoAmI", async (ClaimsPrincipal claims, PostgreSqlContext context) =>
{
    var userId = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await context.Users.FindAsync(userId);
    return user;
}).RequireAuthorization();

app.UseExceptionHandler();

app.Run();

#endregion



