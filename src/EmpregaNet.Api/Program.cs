using EmpregaNet.Api.Configuration;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Service.ServiceRegistration;
using EmpregaNet.Infra;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterCoreDependencies();
builder.AddApiConfiguration();

var builderServices = builder.Services;
builderServices.AddMediator();
builderServices.AddExceptionHandler<GlobalExceptionHandler>();
builderServices.RegisterApplicationDependency();

var app = builder.Build();

#region Configure Pipeline

app.UseApiConfiguration(app.Environment);
app.UseExceptionHandler();
app.Run();

#endregion



