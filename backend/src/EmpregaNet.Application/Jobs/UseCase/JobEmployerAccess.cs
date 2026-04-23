using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Jobs.UseCase;

public sealed class JobEmployerAccess : IJobEmployerAccess
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _currentUser;

    public JobEmployerAccess(UserManager<User> userManager, IHttpCurrentUser currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task EnsureCanManageCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByIdAsync(_currentUser.UserId.ToString());
        if (appUser is null || appUser.IsDeleted)
        {
            throw new ValidationAppException(
                nameof(_currentUser.UserId),
                "Usuário não encontrado.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        if (roles.Contains(RecruitmentRoleNames.Admin))
            return;

        if (appUser.EmployerCompanyId is null)
        {
            throw new ValidationAppException(
                nameof(User.EmployerCompanyId),
                "Seu usuário ainda não está vinculado a uma empresa. Solicite ao administrador.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        if (appUser.EmployerCompanyId.Value != companyId)
        {
            throw new ValidationAppException(
                nameof(companyId),
                "Você só pode gerenciar vagas da empresa à qual está vinculado.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }
    }
}
