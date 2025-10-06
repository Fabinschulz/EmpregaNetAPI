
public static class DependencyInjection
{

    public static void SetupBFFServices(this WebApplication app)
    {
        app.UseHttpsRedirection()
           .UseCors("AllowAll")
           .UseAuthentication()
           .UseAuthorization();

        app.MapControllers();
        app.MapOpenApi();
        app.UseSwaggerSetup();
    }

    public static IServiceCollection RegisterBFFDependencies(this IServiceCollection builder)
    {
        builder.SetupSwaggerDocumentation();
        builder.AddControllers();
        builder.AddOpenApi();
        builder.AddCors(opt =>
           {
               opt.AddPolicy("AllowAll", builder => builder
                  .AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
           });
        return builder;
    }
}
