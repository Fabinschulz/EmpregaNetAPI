using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Api.Controllers.Base;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Api.Controllers.Companies
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : BaseController<CompanyCommand, Company>
    {

        public CompanyController(IMediator mediator, IMemoryService cacheService, IHub sentryHub)
            : base(mediator, cacheService, sentryHub)
        {
        }

    }
}