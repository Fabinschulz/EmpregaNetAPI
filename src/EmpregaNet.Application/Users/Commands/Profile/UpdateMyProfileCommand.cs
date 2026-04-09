using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Users.Commands.Profile;

/// <summary>Dados que o próprio usuário pode alterar (sem mudar tipo/perfil administrativo).</summary>
public sealed record UpdateMyProfileCommand(
    string? Email,
    string? UserName,
    string? PhoneNumber
) : IRequest<UserViewModel>;

public sealed class UpdateMyProfileHandler : IRequestHandler<UpdateMyProfileCommand, UserViewModel>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _httpCurrentUser;
    private readonly IMemoryService _memoryService;
    private readonly ILogger<UpdateMyProfileHandler> _logger;

    public UpdateMyProfileHandler(
        UserManager<User> userManager,
        IHttpCurrentUser httpCurrentUser,
        IMemoryService memoryService,
        ILogger<UpdateMyProfileHandler> logger)
    {
        _userManager = userManager;
        _httpCurrentUser = httpCurrentUser;
        _memoryService = memoryService;
        _logger = logger;
    }

    public async Task<UserViewModel> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_httpCurrentUser.UserId.ToString());
        if (user is null)
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Usuário não encontrado.",
                DomainErrorEnum.USER_NOT_FOUND);
        }

        if (user.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(_httpCurrentUser.UserId),
                "Não é possível atualizar uma conta excluída.",
                DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        }

        await ApplyContactFieldsAsync(_userManager, user, request.Email, request.UserName, request.PhoneNumber);

        user.UpdatedAt = DateTimeOffset.UtcNow;

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            var msg = update.Errors.FirstOrDefault()?.Description ?? "Falha ao salvar os dados.";
            throw new ValidationAppException(nameof(request), msg, DomainErrorEnum.RESOURCE_ERROR);
        }

        _logger.LogInformation("Usuário {UserId} atualizou o próprio perfil.", user.Id);

        _memoryService.Remove(ApplicationCacheKeys.Users.Me(user.Id));
        await _memoryService.RemoveByPatternAsync(ApplicationCacheKeys.Users.AdminPrefix);
        _memoryService.Remove(ApplicationCacheKeys.Candidates.GetById(user.Id));

        return user.ToViewModel();
    }

    /// <summary>
    /// Telefone: <paramref name="phoneNumber"/> nulo = não alterar; caso contrário aplica o valor (vazio limpa).
    /// </summary>
    private static async Task ApplyContactFieldsAsync(
        UserManager<User> userManager,
        User user,
        string? email,
        string? userName,
        string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(userName) && !string.Equals(userName, user.UserName, StringComparison.Ordinal))
        {
            var byName = await userManager.FindByNameAsync(userName);
            if (byName is not null && byName.Id != user.Id)
            {
                throw new ValidationAppException(
                    nameof(userName),
                    "Já existe outro usuário com este nome.",
                    DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
            }

            var setName = await userManager.SetUserNameAsync(user, userName);
            if (!setName.Succeeded)
            {
                var msg = setName.Errors.FirstOrDefault()?.Description ?? "Falha ao atualizar o nome de usuário.";
                throw new ValidationAppException(nameof(userName), msg, DomainErrorEnum.INVALID_PARAMS);
            }
        }

        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var byEmail = await userManager.FindByEmailAsync(email);
            if (byEmail is not null && byEmail.Id != user.Id)
            {
                throw new ValidationAppException(
                    nameof(email),
                    "Já existe outro usuário com este e-mail.",
                    DomainErrorEnum.RESOURCE_ALREADY_EXISTS);
            }

            var setEmail = await userManager.SetEmailAsync(user, email);
            if (!setEmail.Succeeded)
            {
                var msg = setEmail.Errors.FirstOrDefault()?.Description ?? "Falha ao atualizar o e-mail.";
                throw new ValidationAppException(nameof(email), msg, DomainErrorEnum.INVALID_PARAMS);
            }
        }

        if (phoneNumber is not null)
        {
            var phone = string.IsNullOrWhiteSpace(phoneNumber) ? string.Empty : phoneNumber.Trim();
            var setPhone = await userManager.SetPhoneNumberAsync(user, phone);
            if (!setPhone.Succeeded)
            {
                var msg = setPhone.Errors.FirstOrDefault()?.Description ?? "Falha ao atualizar o telefone.";
                throw new ValidationAppException(nameof(phoneNumber), msg, DomainErrorEnum.INVALID_PARAMS);
            }
        }
    }
}
