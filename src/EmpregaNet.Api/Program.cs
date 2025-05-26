using System.Security.Claims;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Configurations;
using EmpregaNet.Infra.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApplication();

var builderServices = builder.Services;
builderServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
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



