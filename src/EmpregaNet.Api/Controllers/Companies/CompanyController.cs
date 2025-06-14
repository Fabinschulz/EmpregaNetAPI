using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Infra.Cache.MemoryService;
using Mediator.Interfaces;
using EmpregaNet.Application.Companies.Command;
using EmpregaNet.Application.Companies.ViewModel;
using Mapper.Interfaces;

namespace EmpregaNet.Api.Controllers.Companies
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : MainController<CreateCompanyCommand, CompanyViewModel>
    {

        public CompanyController(IMemoryService cacheService, IMapperConfigurationProvider configuration)
            : base(cacheService, configuration)
        {
        }

    }
}