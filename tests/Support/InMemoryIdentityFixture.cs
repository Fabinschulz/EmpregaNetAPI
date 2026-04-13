using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EmpregaNet.Tests.Support;

/// <summary>
/// Identity + EF InMemory + handlers reais da camada de aplicação (sem HTTP), partilhado pela coleção "Integration".
/// </summary>
public sealed class InMemoryIdentityFixture : IDisposable
{
    public ServiceProvider Services { get; }
    public Mock<IAccountEmailService> AccountEmail { get; } = new();
    public Mock<IMemoryService> Memory { get; } = new();
    public Mock<IHttpCurrentUser> HttpUser { get; } = new();
    public Mock<IGoogleIdTokenValidator> Google { get; } = new();

    public InMemoryIdentityFixture()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));

        // O nome tem de ser estável: se Guid.NewGuid() ficar dentro do delegate, o EF pode
        // invocar a configuração mais do que uma vez e cada contexto acaba com BD InMemory isolada.
        var inMemoryDatabaseName = $"integration_{Guid.NewGuid():N}";
        services.AddDbContext<PostgreSqlContext>(o =>
            o.UseInMemoryDatabase(inMemoryDatabaseName));

        services.AddIdentity<User, Role>(ConfigureIdentityOptions)
            .AddEntityFrameworkStores<PostgreSqlContext>()
            .AddRoleManager<RoleManager<Role>>()
            .AddSignInManager<SignInManager<User>>()
            .AddUserManager<UserManager<User>>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IOptions<JwtSettings>>(Options.Create(new JwtSettings
        {
            SecretKey = "abcdefghijklmnopqrstuvwxyz0123456789AB",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationHours = 1,
            RefreshTokenExpirationDays = 7
        }));

        services.AddSingleton<IOptions<AppUrlsOptions>>(Options.Create(new AppUrlsOptions
        {
            PublicAppBaseUrl = "https://api.test"
        }));

        AccountEmail.Setup(x => x.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        AccountEmail.Setup(x => x.SendEmailConfirmationLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(AccountEmail.Object);

        Memory.Setup(x => x.Remove(It.IsAny<string>()));
        Memory.Setup(x => x.RemoveByPatternAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        services.AddSingleton(Memory.Object);
        services.AddSingleton(HttpUser.Object);
        services.AddSingleton(Google.Object);

        services.AddScoped<IJwtBuilder, JwtBuilder>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddSingleton<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
        services.AddSingleton<IValidator<LoginUserCommand>, LoginUserCommandValidator>();

        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<ResendEmailConfirmationHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ChangeMyPasswordHandler>();
        services.AddScoped<LoginWithGoogleHandler>();

        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        ctx.Database.EnsureCreated();

        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        if (!rm.RoleExistsAsync("Candidate").GetAwaiter().GetResult())
            rm.CreateAsync(new Role { Name = "Candidate", DataInclusao = DateTimeOffset.UtcNow }).GetAwaiter().GetResult();
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.AllowedForNewUsers = true;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    }

    public void ResetMocks()
    {
        AccountEmail.Invocations.Clear();
        Memory.Invocations.Clear();
        Google.Reset();
    }

    public void Dispose() => Services.Dispose();
}
