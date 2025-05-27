using System.Security.Claims;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Messages;
using EmpregaNet.Domain.Services;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Configurations;
using EmpregaNet.Infra.Persistence.Database;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApplication();

var builderServices = builder.Services;
// builderServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatorHandler).Assembly));

builder.Services.AddMediator(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<Command>();

builderServices.ConfigureServices(builder.Configuration);
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

builderServices.AddHealthChecks()
               .AddCheck<DatabaseCheck>("Database")
               .AddCheck<MemoryServiceCheck>("Cache");

var app = builder.Build();

#region Configure Pipeline

app.UseApiConfiguration(app.Environment);

app.MapGet("/whoAmI", async (ClaimsPrincipal claims, AppDbContext context) =>
{
    var userId = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    var user = await context.Users.FindAsync(userId);
    return user;
}).RequireAuthorization();

// app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseExceptionHandler();

app.Run();

#endregion



