using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Users.Commands;

/// <summary>
/// Autentica com Google a partir do <c>id_token</c> obtido no cliente.
/// </summary>
/// <remarks>
/// <b>Transacional:</b> o provisionamento de uma conta nova escreve em três passos (utilizador,
/// role, login externo). Sem transação, uma falha no meio deixava o utilizador persistido sem
/// credencial utilizável. O <c>TransactionBehavior</c> garante tudo-ou-nada.
/// </remarks>
public sealed record LoginWithGoogleCommand(string IdToken) : IRequest<UserLoggedViewModel>, ITransactional;

public sealed class LoginWithGoogleHandler : IRequestHandler<LoginWithGoogleCommand, UserLoggedViewModel>
{
    private readonly IGoogleIdTokenValidator _googleTokens;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IJwtBuilder _jwtBuilder;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ILogger<LoginWithGoogleHandler> _logger;

    public LoginWithGoogleHandler(
        IGoogleIdTokenValidator googleTokens,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IJwtBuilder jwtBuilder,
        IRefreshTokenService refreshTokens,
        ILogger<LoginWithGoogleHandler> logger)
    {
        _googleTokens = googleTokens;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtBuilder = jwtBuilder;
        _refreshTokens = refreshTokens;
        _logger = logger;
    }

    public async Task<UserLoggedViewModel> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        var payload = await _googleTokens.ValidateAsync(request.IdToken, cancellationToken);
        if (payload is null)
        {
            throw new ValidationAppException(
                nameof(request.IdToken),
                "Token Google inválido ou login social não configurado.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        if (!payload.EmailVerified)
        {
            throw new ValidationAppException(
                nameof(request.IdToken),
                "O e-mail da conta Google não está verificado.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var user = await _userManager.FindByLoginAsync(Constants.ExternalLogin.GoogleProvider, payload.Subject);
        if (user is not null)
        {
            if (user.IsDeleted)
            {
                throw new ValidationAppException(
                    nameof(request.IdToken),
                    "Conta indisponível.",
                    DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
            }

            await EnsureCandidateRoleOrThrowAsync(user, cancellationToken);
            return await BuildTokenWithRefreshAsync(user, cancellationToken);
        }

        user = await _userManager.FindByEmailAsync(payload.Email);
        if (user is not null)
        {
            if (user.IsDeleted)
            {
                throw new ValidationAppException(
                    nameof(request.IdToken),
                    "Conta indisponível.",
                    DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
            }

            var addLogin = await _userManager.AddLoginAsync(user, new UserLoginInfo(Constants.ExternalLogin.GoogleProvider, payload.Subject, "Google"));
            if (!addLogin.Succeeded)
            {
                // Nunca compensar com remoção aqui: esta é uma conta pré-existente, criada por
                // outro caminho. Apagá-la destruiria dados legítimos do utilizador.
                var msg = addLogin.Errors.FirstOrDefault()?.Description ?? "Não foi possível associar o login Google.";
                throw new ValidationAppException(nameof(request.IdToken), msg, DomainErrorEnum.RESOURCE_ERROR);
            }

            _logger.LogInformation("Login Google associado ao usuário existente {UserId}.", user.Id);

            await EnsureCandidateRoleOrThrowAsync(user, cancellationToken);
            return await BuildTokenWithRefreshAsync(user, cancellationToken);
        }

        var userName = await BuildUniqueUserNameAsync(payload.Email);
        user = new User
        {
            UserName = userName,
            Email = payload.Email,
            EmailConfirmed = true,
            UserType = UserTypeEnum.Candidate
        };

        var create = await _userManager.CreateAsync(user);
        if (!create.Succeeded)
        {
            var msg = create.Errors.FirstOrDefault()?.Description ?? "Falha ao criar usuário.";
            throw new ValidationAppException(nameof(request.IdToken), msg, DomainErrorEnum.RESOURCE_CREATION_FAILED);
        }

        var roleResult = await CandidateRoleAssignment.EnsureCandidateRoleAsync(user, _userManager, _roleManager, cancellationToken);
        if (!roleResult.Succeeded)
        {
            var msg = roleResult.Errors.FirstOrDefault()?.Description ?? "Falha ao atribuir a role de candidato.";
            _logger.LogError("Provisionamento via Google abortado: falha ao atribuir a role de candidato. {Errors}", msg);
            throw new ValidationAppException(nameof(request.IdToken), msg, DomainErrorEnum.RESOURCE_CREATION_FAILED);
        }

        var login = await _userManager.AddLoginAsync(user, new UserLoginInfo(Constants.ExternalLogin.GoogleProvider, payload.Subject, "Google"));
        if (!login.Succeeded)
        {
            var msg = login.Errors.FirstOrDefault()?.Description ?? "Falha ao finalizar login social.";
            _logger.LogError("Provisionamento via Google abortado: falha ao registar o login externo. {Errors}", msg);
            throw new ValidationAppException(nameof(request.IdToken), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        _logger.LogInformation("Novo usuário {UserId} criado via Google.", user.Id);
        return await BuildTokenWithRefreshAsync(user, cancellationToken);
    }

    /// <summary>
    /// Garante a role de candidato em contas já existentes.
    /// </summary>
    /// <remarks>
    /// Necessário nos caminhos de conta existente porque a atribuição de role só acontecia na
    /// criação. Se ela tivesse falhado nessa altura, e falhava em silêncio, o utilizador ficava
    /// permanentemente sem role: nenhum login posterior repetia a tentativa.
    /// </remarks>
    private async Task EnsureCandidateRoleOrThrowAsync(User user, CancellationToken cancellationToken)
    {
        var result = await CandidateRoleAssignment.EnsureCandidateRoleAsync(user, _userManager, _roleManager, cancellationToken);

        if (result.Succeeded)
            return;

        var msg = string.Join("; ", result.Errors.Select(e => e.Description));
        _logger.LogError("Falha ao garantir a role de candidato no utilizador {UserId}: {Errors}", user.Id, msg);

        throw new ValidationAppException(
            nameof(LoginWithGoogleCommand.IdToken),
            "Não foi possível concluir o login social.",
            DomainErrorEnum.RESOURCE_ERROR);
    }

    private async Task<UserLoggedViewModel> BuildTokenWithRefreshAsync(User user, CancellationToken cancellationToken)
    {
        var vm = await _jwtBuilder.BuildUserTokenAsync(user);
        vm.RefreshToken = await _refreshTokens.IssueAsync(user.Id, cancellationToken);
        return vm;
    }

    private async Task<string> BuildUniqueUserNameAsync(string email)
    {
        var local = email.Split('@')[0];
        var baseName = string.IsNullOrWhiteSpace(local) ? "user" : local;
        var candidate = baseName;
        var n = 0;
        while (await _userManager.FindByNameAsync(candidate) is not null)
        {
            n++;
            candidate = $"{baseName}{n}";
        }

        return candidate;
    }
}
