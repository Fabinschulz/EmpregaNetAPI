var builder = WebApplication.CreateBuilder(args);
builder.EnvironmentConfig();
builder.Services.RegisterBFFDependencies();

var app = builder.Build();
app.SetupBFFServices();

app.Run();
