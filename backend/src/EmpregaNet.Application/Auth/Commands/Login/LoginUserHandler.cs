using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Utils.CustomValidation;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Auth.Commands;

/// <summary>
/// Autenticação por senha. <paramref name="Identifier"/> aceita <b>CPF ou e-mail</b>.
/// </summary>
public sealed record LoginUserCommand(string Identifier, string Password) : IRequest<UserLoggedViewModel>;

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, UserLoggedViewModel>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtBuilder _jwtBuilder;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ILogger<LoginUserHandler> _logger;

    public LoginUserHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtBuilder jwtBuilder,
        IRefreshTokenService refreshTokens,
        ILogger<LoginUserHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtBuilder = jwtBuilder;
        _refreshTokens = refreshTokens;
        _logger = logger;
    }

    public async Task<UserLoggedViewModel> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await FindByIdentifierAsync(request.Identifier, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            _logger.LogWarning("Tentativa de login para conta inexistente ou excluída.");

            throw new ValidationAppException(
                nameof(request.Identifier),
                "Usuário e/ou senha inválidos.",
                DomainErrorEnum.INVALID_PASSWORD);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login bloqueado por lockout para o usuário {UserId}.", user.Id);

            throw new ValidationAppException(
                nameof(request.Identifier),
                "Conta temporariamente bloqueada por tentativas falhadas. Tente novamente mais tarde.",
                DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        if (result.RequiresTwoFactor)
        {
            _logger.LogWarning("Login exigiu segundo fator, não suportado, para o usuário {UserId}.", user.Id);

            throw new ValidationAppException(
                nameof(request.Identifier),
                "Esta conta requer autenticação em dois passos; contacte o suporte.",
                DomainErrorEnum.UNSUPPORTED_OPERATION);
        }

        if (result.IsNotAllowed)
        {
            _logger.LogWarning("Login não permitido (e-mail não confirmado) para o usuário {UserId}.", user.Id);

            throw new ValidationAppException(
                nameof(request.Identifier),
                "Confirme o seu e-mail antes de iniciar sessão. Verifique a caixa de entrada ou solicite um novo link de confirmação.",
                DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Senha inválida para o usuário {UserId}.", user.Id);

            throw new ValidationAppException(
                nameof(request.Password),
                "Usuário e/ou senha inválidos.",
                DomainErrorEnum.INVALID_PASSWORD);
        }

        _logger.LogInformation("Login bem-sucedido para o usuário {UserId}.", user.Id);

        var vm = await _jwtBuilder.BuildUserTokenAsync(user);
        vm.RefreshToken = await _refreshTokens.IssueAsync(user.Id, cancellationToken);
        return vm;
    }

    /// <summary>
    /// Decide entre CPF e e-mail pelo formato do valor e devolve o utilizador correspondente.
    /// </summary>
    private async Task<User?> FindByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        var candidate = identifier.Trim();
        var cpf = BrazilianDocument.NormalizeCpf(candidate);

        if (cpf.Length == BrazilianDocument.CpfLength)
        {
            return BrazilianDocument.IsValidCpf(cpf)
                ? await _userManager.Users.FirstOrDefaultAsync(u => u.Cpf == cpf, cancellationToken)
                : null;
        }

        return candidate.Contains('@')
            ? await _userManager.FindByEmailAsync(candidate)
            : null;
    }
}
