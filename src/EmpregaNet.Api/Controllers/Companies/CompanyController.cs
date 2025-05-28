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

        // [HttpPost]
        // [ProducesResponseType(typeof(Company), StatusCodes.Status201Created)]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public override async Task<IActionResult> Create([FromBody] CompanyCommand comm)
        // {

        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     var command = new CreateCommand<CompanyCommand, Company>(comm);
        //     var result = await _mediator.Send(command);

        //     await InvalidateCacheForEntity();
        //     return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        // }


        protected override long GetEntityId(Company entity) => entity.Id;
    }
}