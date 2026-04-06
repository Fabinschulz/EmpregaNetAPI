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
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtBuilder jwtBuilder,
        IValidator<LoginUserCommand> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtBuilder = jwtBuilder;
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

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new ValidationAppException(
                nameof(request.Password),
                "Usuário e/ou senha inválidos.",
                DomainErrorEnum.INVALID_PASSWORD);
        }

        return await _jwtBuilder.BuildUserTokenAsync(user);
    }
}
