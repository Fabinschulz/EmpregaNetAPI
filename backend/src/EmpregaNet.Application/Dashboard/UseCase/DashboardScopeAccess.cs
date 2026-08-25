using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EmpregaNet.Application.Dashboard.UseCase;

public sealed record DashboardResolvedScope(DashboardScope Scope, string? CompanyName);

public interface IDashboardScopeAccess
{
    Task<DashboardResolvedScope> ResolveAsync(long? requestedCompanyId, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IDashboardScopeAccess"/>
public sealed class DashboardScopeAccess : IDashboardScopeAccess
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _currentUser;
    private readonly ICompanyRepository _companyRepository;

    public DashboardScopeAccess(
        UserManager<User> userManager,
        IHttpCurrentUser currentUser,
        ICompanyRepository companyRepository)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _companyRepository = companyRepository;
    }

    public async Task<DashboardResolvedScope> ResolveAsync(
        long? requestedCompanyId,
        CancellationToken cancellationToken = default)
    {
        RecruitmentAccess.EnsureRecruitmentStaff(_currentUser);

        var user = await _userManager.FindByIdAsync(_currentUser.UserId.ToString());
        if (user is null || user.IsDeleted)
        {
            throw ValidationAppException.ForBusinessRule(
                "Usuário não encontrado.",
                DomainErrorEnum.USER_NOT_FOUND);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(RecruitmentRoleNames.Admin, StringComparer.OrdinalIgnoreCase);

        if (isAdmin)
        {
            return requestedCompanyId is { } adminCompanyId
                ? await ScopeForCompanyAsync(adminCompanyId, cancellationToken)
                : new DashboardResolvedScope(DashboardScope.Platform, null);
        }

        if (user.EmployerCompanyId is not { } ownCompanyId)
        {
            throw ValidationAppException.ForBusinessRule(
                "Seu usuário ainda não está vinculado a uma empresa. Solicite ao administrador.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        if (requestedCompanyId is { } requested && requested != ownCompanyId)
        {
            throw ValidationAppException.ForBusinessRule(
                "Você só pode consultar as métricas da empresa à qual está vinculado.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        return await ScopeForCompanyAsync(ownCompanyId, cancellationToken);
    }

    private async Task<DashboardResolvedScope> ScopeForCompanyAsync(long companyId, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null || company.IsDeleted)
        {
            throw new ValidationAppException(
                "companyId",
                $"Empresa com ID '{companyId}' não encontrada.",
                DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
        }

        return new DashboardResolvedScope(new DashboardScope(companyId), company.CompanyName);
    }
}
