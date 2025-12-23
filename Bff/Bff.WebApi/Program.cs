using Bff.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseHttpsRedirection();
app.Run();