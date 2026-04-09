using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Common;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Jobs;

/// <summary>
/// Vagas: leitura pública (descoberta para candidatos); criação/edição/exclusão/fechamento apenas para perfis de recrutamento.
/// </summary>
[Route("api/[controller]")]
public class JobsController : MainController<CreateJobCommand, UpdateJobCommand, JobViewModel>
{
    public JobsController(IMemoryService cacheService) : base(cacheService)
    {
    }

    [AllowAnonymous]
    [HttpGet]
    public override Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isActive = null)
        => base.GetAll(page, size, orderBy, isDeleted, isActive);

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public override Task<IActionResult> GetById([FromRoute] long id) => base.GetById(id);

    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] CreateJobCommand entity)
        => base.Create(entity);

    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpPut("{id:long}")]
    public override Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateJobCommand entity)
        => base.Update(id, entity);

    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpDelete("{id:long}")]
    public override Task<IActionResult> Delete([FromRoute] long id) => base.Delete(id);

    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
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
