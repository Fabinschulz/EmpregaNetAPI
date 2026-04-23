using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Application.Users.Commands;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string PasswordConfirmation,
    string? PhoneNumber
) : IRequest<long>;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, long>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IAccountEmailService _accountEmail;
    private readonly AppUrlsOptions _urls;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IAccountEmailService accountEmail,
        IOptions<AppUrlsOptions> urlsOptions,
        ILogger<RegisterUserHandler> logger,
        IValidator<RegisterUserCommand> validator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _accountEmail = accountEmail;
        _urls = urlsOptions.Value;
        _logger = logger;
        _validator = validator;
    }

    public async Task<long> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Password != request.PasswordConfirmation)
        {
            throw new ValidationAppException(
                nameof(request.PasswordConfirmation),
                "A confirmação de senha não confere.",
                DomainErrorEnum.INVALID_PARAMS);
        }

        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
        {
            throw new ValidationAppException(
                nameof(request.Email),
                "Já existe um usuário com este e-mail.",
                DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
        }

        var existingByUsername = await _userManager.FindByNameAsync(request.Username);
        if (existingByUsername is not null)
        {
            throw new ValidationAppException(
                nameof(request.Username),
                "Já existe um usuário com este nome.",
                DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
        }

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserType = UserTypeEnum.Candidate
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Falha ao criar usuário.";
            throw new ValidationAppException(nameof(request.Username), errorMessage, DomainErrorEnum.RESOURCE_CREATION_FAILED);
        }

        await CandidateRoleAssignment.EnsureCandidateRoleAsync(user, _userManager, _roleManager, cancellationToken);

        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var safeToken = Uri.EscapeDataString(token);
            var baseUrl = _urls.PublicAppBaseUrl.TrimEnd('/');
            var path = _urls.EmailConfirmationPath.StartsWith('/') ? _urls.EmailConfirmationPath : "/" + _urls.EmailConfirmationPath;
            var link = $"{baseUrl}{path}?userId={user.Id}&token={safeToken}";
            await _accountEmail.SendEmailConfirmationLinkAsync(user.Email!, link, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de confirmação para o utilizador {UserId}.", user.Id);
        }

        return user.Id;
    }
}
