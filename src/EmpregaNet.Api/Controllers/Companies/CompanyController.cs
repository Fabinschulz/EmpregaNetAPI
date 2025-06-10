using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Infra.Cache.MemoryService;
using EmpregaNet.Domain.Components.Mediator.Interfaces;
using EmpregaNet.Application.Companies.Command;

namespace EmpregaNet.Api.Controllers.Companies
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : MainController<CreateCompanyCommand, Company>
    {

        public CompanyController(IMediator mediator, IMemoryService cacheService, IHub sentryHub)
            : base(mediator, cacheService, sentryHub)
        {
        }

    }
}