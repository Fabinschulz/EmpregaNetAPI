using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Users.Commands;

public sealed record LoginWithGoogleCommand(string IdToken) : IRequest<UserLoggedViewModel>;

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

            return await _jwtBuilder.BuildUserTokenAsync(user);
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
                var msg = addLogin.Errors.FirstOrDefault()?.Description ?? "Não foi possível associar o login Google.";
                throw new ValidationAppException(nameof(request.IdToken), msg, DomainErrorEnum.RESOURCE_ERROR);
            }

            _logger.LogInformation("Login Google associado ao usuário existente {UserId}.", user.Id);
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

        await CandidateRoleAssignment.EnsureCandidateRoleAsync(user, _userManager, _roleManager, cancellationToken);

        var login = await _userManager.AddLoginAsync(user, new UserLoginInfo(Constants.ExternalLogin.GoogleProvider, payload.Subject, "Google"));
        if (!login.Succeeded)
        {
            _logger.LogError("Usuário {UserId} criado via Google mas falha ao registrar login externo.", user.Id);
            var msg = login.Errors.FirstOrDefault()?.Description ?? "Falha ao finalizar login social.";
            throw new ValidationAppException(nameof(request.IdToken), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        _logger.LogInformation("Novo usuário {UserId} criado via Google.", user.Id);
        return await BuildTokenWithRefreshAsync(user, cancellationToken);
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
