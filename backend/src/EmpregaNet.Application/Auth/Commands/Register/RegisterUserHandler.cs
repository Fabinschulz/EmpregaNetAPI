using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.Events;
using EmpregaNet.Application.Users.Identity;
using EmpregaNet.Application.Utils.CustomValidation;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Auth.Commands;

/// <summary>
/// Registo por e-mail e senha.
/// </summary>
/// <remarks>
/// <b>Transacional:</b> a criação do utilizador e a atribuição da role passam a ser tudo-ou-nada.
/// Antes, uma falha na role deixava o utilizador criado <b>sem role nenhuma</b>, de forma
/// permanente, nenhum login posterior repetia a tentativa.
///
/// <para><b>O e-mail de confirmação não sai daqui.</b> O handler apenas declara
/// <see cref="UserRegistered"/> na fila de eventos de domínio; o envio corre no
/// <c>UserRegisteredEmailHandler</c>, já depois do commit. Dentro da transacção havia dois modos de
/// falha que nenhum rollback desfaz: um commit falhado deixava um link de confirmação enviado para
/// um registo que não existe, e uma reexecução da tentativa pela estratégia de retry mandava um
/// segundo e-mail.</para>
/// </remarks>
public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Cpf,
    string Password,
    string PasswordConfirmation,
    string? PhoneNumber
) : IRequest<long>, ITransactional;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, long>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IDomainEventQueue _domainEvents;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IDomainEventQueue domainEvents,
        ILogger<RegisterUserHandler> logger,
        IValidator<RegisterUserCommand> validator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _domainEvents = domainEvents;
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

        var cpf = BrazilianDocument.NormalizeCpf(request.Cpf);
        var existingByCpf = await _userManager.Users.AnyAsync(u => u.Cpf == cpf, cancellationToken);
        if (existingByCpf)
        {
            _logger.LogWarning("Registo recusado: o CPF informado já está associado a uma conta.");

            throw ValidationAppException.ForBusinessRule(
                "Não foi possível concluir o cadastro com os dados informados. Entre em contato com o suporte.",
                DomainErrorEnum.RESOURCE_CREATION_FAILED);
        }

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            Cpf = cpf,
            PhoneNumber = request.PhoneNumber,
            UserType = UserTypeEnum.Candidate
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Falha ao criar usuário.";
            throw new ValidationAppException(nameof(request.Username), errorMessage, DomainErrorEnum.RESOURCE_CREATION_FAILED);
        }


        var roleResult = await CandidateRoleAssignment.EnsureCandidateRoleAsync(user, _userManager, _roleManager, cancellationToken);
        if (!roleResult.Succeeded)
        {
            var roleErrors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError(
                "Registo abortado: falha ao atribuir a role {Role} ao utilizador {Username}: {Errors}",
                CandidateRoleAssignment.RoleName, request.Username, roleErrors);

            throw new ValidationAppException(
                nameof(request.Username),
                "Não foi possível concluir o registo.",
                DomainErrorEnum.RESOURCE_CREATION_FAILED);
        }

        _domainEvents.Enqueue(new UserRegistered(user.Id));

        return user.Id;
    }
}
