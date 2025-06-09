using System.Security.Claims;
using EmpregaNet.Api.Configurations;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Service;
using EmpregaNet.Domain.Services;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApplication();
builder.AddApiConfiguration();

var builderServices = builder.Services;
builderServices.ConfigureServices(builder.Configuration);
builderServices.AddMediator(typeof(Program).Assembly);
builderServices.AddExceptionHandler<GlobalExceptionHandler>();
builderServices.AddApplication(); // Registra serviços e handlers da camada de Application (ex: MediatR, automappers, etc.)

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



