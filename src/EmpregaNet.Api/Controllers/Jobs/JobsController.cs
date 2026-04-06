using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Domain.Common;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Jobs
{
    [Route("api/[controller]")]
    [Authorize]
    public class JobsController : MainController<CreateJobCommand, UpdateJobCommand, JobViewModel>
    {

        public JobsController(IMemoryService cacheService) : base(cacheService)
        {
        }

        [HttpPut("{id:long}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
        public async Task<IActionResult> Close([FromRoute] long id)
        {
            await _mediator.Send(new CloseJobCommand(id));
            await InvalidateCacheForEntity(id);
            return Ok("Vaga encerrada com sucesso.");
        }
    }

}