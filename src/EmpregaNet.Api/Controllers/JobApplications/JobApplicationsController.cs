using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Utils;
using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Application.JobApplications.Queries;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.JobApplications;

/// <summary>
/// Candidaturas a vagas: candidatos criam e consultam as suas; recrutamento lista todas, por vaga e altera estado.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class JobApplicationsController : MainController<ApplyToJobCommand, ChangeJobApplicationStatusCommand, JobApplicationViewModel>
{
    public JobApplicationsController(IMemoryService cacheService)
        : base(cacheService)
    {
    }

    /// <summary>Candidata o utilizador autenticado a uma vaga.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public override async Task<IActionResult> Create([FromBody] ApplyToJobCommand command)
    {
        var id = await _mediator.Send(new CreateCommand<ApplyToJobCommand>(command));
        await InvalidateCacheForEntity(id);
        return Created($"api/jobapplications/{id}", $"Candidatura criada com sucesso. ID: {id}");
    }

    /// <summary>Lista todas as candidaturas (paginação; apenas recrutamento).</summary>
    [HttpGet]
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    public override Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isActive = null)
        => base.GetAll(page, size, orderBy, isDeleted, isActive);

    /// <summary>Lista as candidaturas do utilizador autenticado.</summary>
    [HttpGet("mine")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListDataPagination<JobApplicationViewModel>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? status = null,
        [FromQuery] string? orderBy = null)
    {
        var cacheKey = ApplicationCacheKeys.JobApplications.Mine(page, size, status, orderBy);
        var cachedData = await _cacheService.GetValueAsync<ListDataPagination<JobApplicationViewModel>>(cacheKey);
        if (cachedData is not null) return Ok(cachedData);

        var result = await _mediator.Send(new GetMyJobApplicationsQuery(page, size, status, orderBy));
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <summary>Lista candidaturas associadas a uma vaga (apenas recrutamento).</summary>
    [HttpGet("job/{jobId:long}")]
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListDataPagination<JobApplicationViewModel>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetByJob(
        [FromRoute] long jobId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? status = null,
        [FromQuery] string? orderBy = null)
    {
        var cacheKey = ApplicationCacheKeys.JobApplications.ByJob(jobId, page, size, status, orderBy);
        var cachedData = await _cacheService.GetValueAsync<ListDataPagination<JobApplicationViewModel>>(cacheKey);
        if (cachedData is not null) return Ok(cachedData);

        var result = await _mediator.Send(new GetJobApplicationsByJobIdQuery(jobId, page, size, status, orderBy));
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <summary>Atualiza o estado de uma candidatura (apenas recrutamento).</summary>
    [HttpPut("{id:long}")]
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobApplicationViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public override Task<IActionResult> Update([FromRoute] long id, [FromBody] ChangeJobApplicationStatusCommand entity)
        => base.Update(id, entity);

    /// <summary>Remove uma candidatura (apenas recrutamento).</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    public override Task<IActionResult> Delete([FromRoute] long id) => base.Delete(id);

    protected override async Task InvalidateCacheForEntity(long id = default)
    {
        await base.InvalidateCacheForEntity(id);
        await _cacheService.RemoveByPatternAsync(ApplicationCacheKeys.JobApplications.MinePrefix);
        await _cacheService.RemoveByPatternAsync(ApplicationCacheKeys.JobApplications.ByJobPrefix);
    }
}
