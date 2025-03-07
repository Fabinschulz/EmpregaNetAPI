using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra;

var builder = WebApplication.CreateBuilder(args);
builder.AddDependencyInjection();
var builderServices = builder.Services;
builderServices.ConfigureServices();

var app = builder.Build();

// builderServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmpregaNet Api v1"));
app.MapSwagger();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityApi<User>();

app.MapGet("/", () => "Hello World!");

app.Run();
