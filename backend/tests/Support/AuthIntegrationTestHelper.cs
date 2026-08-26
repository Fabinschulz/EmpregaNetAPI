using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Support;

internal static class AuthIntegrationTestHelper
{
    public const string DefaultPassword = "Abcd@123";

    /// <summary>
    /// Regista, confirma e-mail e devolve o id (utilizador pronto para login por senha).
    /// </summary>
    /// <param name="cpf">
    /// CPF a gravar; nulo gera um válido. Passe um valor explícito quando o teste precisar de
    /// autenticar por CPF e portanto de o conhecer.
    /// </param>
    public static async Task<long> RegisterConfirmedUserAsync(
        IServiceProvider services,
        string email,
        string usernamePrefix,
        string? cpf = null,
        CancellationToken ct = default)
    {
        var username = TestDataFactory.UniqueUsername(usernamePrefix);
        using var scope = services.CreateScope();
        var register = scope.ServiceProvider.GetRequiredService<RegisterUserHandler>();
        var id = await register.Handle(
            new RegisterUserCommand(
                username,
                email,
                cpf ?? TestDataFactory.UniqueCpf(),
                DefaultPassword,
                DefaultPassword,
                TestDataFactory.UniqueBrazilianCell()),
            ct);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var entity = await users.FindByIdAsync(id.ToString());
        if (entity is null)
            throw new InvalidOperationException("Utilizador não encontrado após registo.");

        var token = await users.GenerateEmailConfirmationTokenAsync(entity);
        var confirm = scope.ServiceProvider.GetRequiredService<ConfirmEmailHandler>();
        await confirm.Handle(new ConfirmEmailCommand(id, token), ct);
        return id;
    }
}
