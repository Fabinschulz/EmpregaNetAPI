using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Admin.Company.Queries;

/// <summary>
/// Lista as empresas que o usuário atual pode selecionar ao publicar/editar uma vaga:
/// Admin vê todas as empresas ativas; recrutador/gestor vê apenas a empresa vinculada (<see cref="User.EmployerCompanyId"/>).
/// </summary>
public sealed record GetSelectableCompaniesQuery() : IRequest<IReadOnlyList<CompanyOptionViewModel>>;

public sealed class GetSelectableCompaniesHandler
    : IRequestHandler<GetSelectableCompaniesQuery, IReadOnlyList<CompanyOptionViewModel>>
{
    private const int MaxCompanies = 500;

    private readonly ICompanyRepository _companyRepository;
    private readonly UserManager<User> _userManager;
    private readonly IHttpCurrentUser _currentUser;
    private readonly ILogger<GetSelectableCompaniesHandler> _logger;

    public GetSelectableCompaniesHandler(
        ICompanyRepository companyRepository,
        UserManager<User> userManager,
        IHttpCurrentUser currentUser,
        ILogger<GetSelectableCompaniesHandler> logger)
    {
        _companyRepository = companyRepository;
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CompanyOptionViewModel>> Handle(
        GetSelectableCompaniesQuery request,
        CancellationToken cancellationToken)
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
        {
            var page = await _companyRepository.GetAllAsync(
                cancellationToken,
                page: 1,
                size: MaxCompanies,
                orderBy: null,
                isDeleted: false);

            return page.Data.Select(c => c.ToOptionViewModel()).ToList();
        }

        if (appUser.EmployerCompanyId is null)
        {
            _logger.LogInformation("Usuário {UserId} sem empresa vinculada ao listar empresas selecionáveis.", appUser.Id);
            return Array.Empty<CompanyOptionViewModel>();
        }

        var company = await _companyRepository.GetByIdAsync(appUser.EmployerCompanyId.Value, cancellationToken);
        if (company is null || company.IsDeleted)
        {
            return Array.Empty<CompanyOptionViewModel>();
        }

        return new[] { company.ToOptionViewModel() };
    }
}
