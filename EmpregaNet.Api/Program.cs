using System.Security.Claims;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApplication();

var builderServices = builder.Services;
builderServices.ConfigureServices(builder.Configuration);
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
builderServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


#region Configure Pipeline

app.UseApiConfiguration(app.Environment);

app.MapGet("/whoAmI", (ClaimsPrincipal user) =>
{
    return user.Identity?.IsAuthenticated == true
        ? Results.Ok(new { Name = user.Identity.Name, Claims = user.Claims.Select(c => new { Type = c.Type, Value = c.Value }) })
        : Results.Unauthorized();
});

// app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<User>();

app.Run();

#endregion



