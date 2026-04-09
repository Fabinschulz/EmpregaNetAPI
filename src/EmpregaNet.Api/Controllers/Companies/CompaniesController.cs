using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Application.Utils;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Application.Admin.Company.Commands;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EmpregaNet.Api.Controllers.Companies
{
    [Route("api/[controller]")]
    [Authorize(Policy = Constants.AuthPolicies.Administrador)]
    public class CompaniesController : MainController<CreateCompanyCommand, UpdateCompanyCommand, CompanyViewModel>
    {

        public CompaniesController(IMemoryService cacheService) : base(cacheService)
        {
        }

    }
}