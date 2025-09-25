using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Domain.Interfaces;

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