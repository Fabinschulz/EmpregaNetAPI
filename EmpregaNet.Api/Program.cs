using EmpregaNet.Api.Middleware;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApplication();

var builderServices = builder.Services;
builderServices.ConfigureServices();
builderServices.AddControllers();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
builderServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


#region Configure Pipeline

app.UseApiConfiguration(app.Environment);
// app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.Run();

#endregion



