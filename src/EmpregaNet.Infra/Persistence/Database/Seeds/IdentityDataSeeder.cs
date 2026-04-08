using System.Security.Claims;
using EmpregaNet.Application.Auth;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Infra.Persistence.Database;

/// <summary>
/// Garante roles padrão e um usuário administrador inicial (idempotente).
/// </summary>
public static class IdentityDataSeeder
{
    /// <summary>
    /// Alinhado às policies de autorização (Admin, Recrutamento) e ao registro de candidatos.
    /// </summary>
    public static readonly string[] SystemRoleNames =
    [
        "Admin",
        "Candidate",
        "Recruiter",
        "Manager",
        "Interviewer",
        "Analyst"
    ];

    public static async Task SeedAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentityDataSeeder));

        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(roleManager, logger, cancellationToken);
        await SeedAdminRolePermissionsAsync(roleManager, logger, cancellationToken);
        await SeedAdminUserAsync(userManager, configuration, logger, cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger, CancellationToken cancellationToken)
    {
        foreach (var roleName in SystemRoleNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = new Role { Name = roleName, DataInclusao = DateTimeOffset.UtcNow };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                logger.LogError("Falha ao criar role {Role}: {Errors}", roleName, msg);
                throw new InvalidOperationException($"Seed: não foi possível criar a role '{roleName}': {msg}");
            }

            logger.LogInformation("Role criada pelo seed: {Role}", roleName);
        }
    }

    /// <summary>
    /// Persiste em Identity (<see cref="PermissionClaims.IdentityRolePermission"/>) todas as combinações de recurso e ação para a role Admin;
    /// o JWT é montado depois com a claim compacta <see cref="PermissionClaims.JwtScopes"/>.
    /// </summary>
    private static async Task SeedAdminRolePermissionsAsync(
        RoleManager<Role> roleManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole is null)
        {
            logger.LogWarning("Seed de permissões ignorado: role Admin não encontrada.");
            return;
        }

        var existingClaims = await roleManager.GetClaimsAsync(adminRole);
        var existingPermissionValues = existingClaims
            .Where(c => c.Type == PermissionClaims.IdentityRolePermission)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var resource in Enum.GetValues<PermissionResourceEnum>())
        {
            foreach (var type in Enum.GetValues<PermissionTypeEnum>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var value = $"{resource}:{type}";
                if (existingPermissionValues.Contains(value))
                    continue;

                var result = await roleManager.AddClaimAsync(adminRole, new Claim(PermissionClaims.IdentityRolePermission, value));
                if (!result.Succeeded)
                {
                    var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                    logger.LogError("Falha ao adicionar permissão {Permission} à role Admin: {Errors}", value, msg);
                    throw new InvalidOperationException($"Seed: permissão '{value}' na role Admin: {msg}");
                }

                existingPermissionValues.Add(value);
            }
        }

        logger.LogInformation("Permissões da role Admin verificadas/atualizadas pelo seed.");
    }

    private static async Task SeedAdminUserAsync(
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var options = configuration.GetSection(SeedOptions.SectionName).Get<SeedOptions>() ?? new SeedOptions();

        if (string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            logger.LogWarning(
                "Seed: senha de administrador não configurada ({Section}__{PasswordField}). Usuário admin não será criado.",
                SeedOptions.SectionName,
                nameof(SeedOptions.AdminPassword));
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var email = options.AdminEmail.Trim();
        var userName = string.IsNullOrWhiteSpace(options.AdminUserName) ? email : options.AdminUserName.Trim();

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (!await userManager.IsInRoleAsync(existing, "Admin"))
            {
                var addRole = await userManager.AddToRoleAsync(existing, "Admin");
                if (!addRole.Succeeded)
                {
                    var msg = string.Join("; ", addRole.Errors.Select(e => e.Description));
                    logger.LogWarning("Usuário {Email} existe mas não estava na role Admin; falha ao associar: {Errors}", email, msg);
                }
                else
                    logger.LogInformation("Role Admin associada ao usuário existente {Email}.", email);
            }

            return;
        }

        var admin = new User
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            UserType = UserTypeEnum.Admin
        };

        var createResult = await userManager.CreateAsync(admin, options.AdminPassword);
        if (!createResult.Succeeded)
        {
            var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
            logger.LogError("Falha ao criar usuário administrador: {Errors}", msg);
            throw new InvalidOperationException($"Seed: não foi possível criar o administrador: {msg}");
        }

        var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
        if (!roleResult.Succeeded)
        {
            var msg = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            logger.LogError("Administrador criado mas falha ao associar role Admin: {Errors}", msg);
            throw new InvalidOperationException($"Seed: usuário admin criado sem role Admin: {msg}");
        }

        logger.LogInformation(
            "Usuário administrador criado pelo seed (login: {UserName}, e-mail: {Email}). Altere a senha em produção.",
            userName,
            email);
    }
}
