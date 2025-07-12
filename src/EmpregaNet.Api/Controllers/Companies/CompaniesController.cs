using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Companies.ViewModel;

namespace EmpregaNet.Api.Controllers.Companies
{
    [Route("api/[controller]")]
    public class CompaniesController : MainController<CompanyCommand, CompanyViewModel>
    {

        public CompaniesController(IMemoryService cacheService) : base(cacheService)
        {
        }

    }
}