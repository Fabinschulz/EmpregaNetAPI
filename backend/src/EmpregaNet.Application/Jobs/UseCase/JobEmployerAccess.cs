using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Abstraction;
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
        var (isAdmin, ownCompanyId) = await ResolveCurrentStaffScopeAsync();
        if (isAdmin)
            return;

        if (ownCompanyId is null)
        {
            throw ValidationAppException.ForBusinessRule(
                "Seu usuário ainda não está vinculado a uma empresa. Solicite ao administrador.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        if (ownCompanyId.Value != companyId)
        {
            throw ValidationAppException.ForBusinessRule(
                "Você só pode gerenciar vagas da empresa à qual está vinculado.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }
    }

    public async Task<long?> ResolveCompanyScopeAsync(CancellationToken cancellationToken = default)
    {
        var (isAdmin, ownCompanyId) = await ResolveCurrentStaffScopeAsync();
        if (isAdmin)
            return null;

        if (ownCompanyId is null)
        {
            throw ValidationAppException.ForBusinessRule(
                "Seu usuário ainda não está vinculado a uma empresa. Solicite ao administrador.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        return ownCompanyId;
    }

    private async Task<(bool IsAdmin, long? CompanyId)> ResolveCurrentStaffScopeAsync()
    {
        var appUser = await _userManager.FindByIdAsync(_currentUser.UserId.ToString());
        if (appUser is null || appUser.IsDeleted)
        {
            throw ValidationAppException.ForBusinessRule(
                "Usuário não encontrado.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        if (roles.Contains(RecruitmentRoleNames.Admin))
            return (true, null);

        return (false, appUser.EmployerCompanyId);
    }
}
