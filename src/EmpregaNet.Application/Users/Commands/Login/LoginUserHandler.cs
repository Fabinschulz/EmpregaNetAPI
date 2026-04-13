using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Users.Commands;

public sealed record LoginUserCommand(string Login, string Password) : IRequest<UserLoggedViewModel>;

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, UserLoggedViewModel>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtBuilder _jwtBuilder;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtBuilder jwtBuilder,
        IRefreshTokenService refreshTokens,
        IValidator<LoginUserCommand> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtBuilder = jwtBuilder;
        _refreshTokens = refreshTokens;
        _validator = validator;
    }

    public async Task<UserLoggedViewModel> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        User? user;
        if (request.Login.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(request.Login);
        }
        else
        {
            user = await _userManager.FindByNameAsync(request.Login);
        }

        if (user is null || user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(request.Login),
                "Usuário e/ou senha inválidos.",
                DomainErrorEnum.INVALID_PASSWORD);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            throw new ValidationAppException(
                nameof(request.Login),
                "Conta temporariamente bloqueada por tentativas falhadas. Tente novamente mais tarde.",
                DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        if (result.RequiresTwoFactor)
        {
            throw new ValidationAppException(
                nameof(request.Login),
                "Esta conta requer autenticação em dois passos; contacte o suporte.",
                DomainErrorEnum.UNSUPPORTED_OPERATION);
        }

        if (result.IsNotAllowed)
        {
            throw new ValidationAppException(
                nameof(request.Login),
                "Confirme o seu e-mail antes de iniciar sessão. Verifique a caixa de entrada ou solicite um novo link de confirmação.",
                DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        }

        if (!result.Succeeded)
        {
            throw new ValidationAppException(
                nameof(request.Password),
                "Usuário e/ou senha inválidos.",
                DomainErrorEnum.INVALID_PASSWORD);
        }

        var vm = await _jwtBuilder.BuildUserTokenAsync(user);
        vm.RefreshToken = await _refreshTokens.IssueAsync(user.Id, cancellationToken);
        return vm;
    }
}
