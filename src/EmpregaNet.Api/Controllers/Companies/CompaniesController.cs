using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Companies.ViewModel;
using Mapper.Interfaces;

namespace EmpregaNet.Api.Controllers.Companies
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : MainController<CreateCompanyCommand, CompanyViewModel>
    {

        public CompaniesController(IMemoryService cacheService, IMapperConfigurationProvider configuration)
            : base(cacheService, configuration)
        {
        }

    }
}