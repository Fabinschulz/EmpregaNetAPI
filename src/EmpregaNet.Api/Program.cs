using EmpregaNet.Api.Middleware;
using EmpregaNet.Infra;

var builder = WebApplication.CreateBuilder(args);
builder.EnvironmentConfig();

var builderServices = builder.Services;
builderServices.AddMediator();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();

// Register dependencies
builderServices.RegisterApiDependencies();
builderServices.RegisterApplicationDependencies();
builder.RegisterCoreDependencies();

var app = builder.Build();

#region Configure Pipeline

app.SetupApiServices();
app.UseExceptionHandler();
app.Run();

#endregion



