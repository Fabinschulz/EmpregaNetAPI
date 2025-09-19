using EmpregaNet.Api.Configuration;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Service.ServiceRegistration;
using EmpregaNet.Domain.Components.Mediator.Extensions;
using EmpregaNet.Infra;
using EmpregaNet.Infra.Configurations;

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
app.UseExceptionHandler();
app.Run();

#endregion



