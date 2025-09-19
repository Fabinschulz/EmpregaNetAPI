using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Companies.Command;

namespace EmpregaNet.Api.Controllers.Companies
{
    [Route("api/[controller]")]
    public class CompaniesController : MainController<CreateCompanyCommand, UpdateCompanyCommand, CompanyViewModel>
    {

        public CompaniesController(IMemoryService cacheService) : base(cacheService)
        {
        }

    }
}