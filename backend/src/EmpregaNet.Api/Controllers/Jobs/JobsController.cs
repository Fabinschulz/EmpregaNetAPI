using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Common;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Admin.Company.Queries;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Api.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EmpregaNet.Api.Controllers.Jobs;

/// <summary>
/// Vagas: leitura pública (descoberta para candidatos); criação/edição/exclusão/fechamento apenas para perfis de recrutamento.
/// </summary>
[Route("api/[controller]")]
public class JobsController : MainController<CreateJobCommand, UpdateJobCommand, JobViewModel>
{
    public JobsController(IOutputCacheManager cacheService) : base(cacheService)
    {
    }

    /// <summary>Assinatura genérica da base, suprimida do routing: vagas expõem a action com o filtro isActive abaixo.</summary>
    [NonAction]
    public override Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string? search = null)
        => GetAll(page, size, orderBy, isDeleted, null, search);

    /// <summary>Retorna uma lista paginada de vagas, com filtros opcionais (isActive: ativas/encerradas; busca por título/descrição).</summary>
    [AllowAnonymous]
    [HttpGet]
    [OutputCache(PolicyName = OutputCachePolicies.PublicCatalog)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetAllQuery<JobViewModel>(page, size, orderBy, isDeleted, isActive, search));
        return Ok(result);
    }

    /// <summary>Retorna os detalhes de uma vaga específica por ID, incluindo título, descrição, empresa, localização, requisitos, etc.</summary>
    [AllowAnonymous]
    [HttpGet("{id:long}")]
    [OutputCache(PolicyName = OutputCachePolicies.PublicCatalog)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobViewModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public override Task<IActionResult> GetById([FromRoute] long id) => base.GetById(id);

    /// <summary>
    /// Lista as empresas que o usuário atual pode selecionar ao publicar/editar uma vaga
    /// (Admin: todas as ativas; recrutador/gestor: apenas a empresa vinculada). Apenas para perfis de recrutamento.
    /// </summary>
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpGet("selectable-companies")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<CompanyOptionViewModel>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    public async Task<IActionResult> GetSelectableCompanies()
    {
        var result = await _mediator.Send(new GetSelectableCompaniesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova vaga com os dados fornecidos (título, descrição, empresa, localização, requisitos, etc.). Apenas para perfis de recrutamento.
    /// </summary>
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    public override Task<IActionResult> Create([FromBody] CreateJobCommand entity)
        => base.Create(entity);

    /// <summary>
    /// Atualiza os dados de uma vaga existente por ID. Apenas para perfis de recrutamento.
    /// </summary>
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public override Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateJobCommand entity)
        => base.Update(id, entity);

    /// <summary>
    /// Exclui uma vaga por ID. Apenas para perfis de recrutamento.
    /// </summary>
    [Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public override Task<IActionResult> Delete([FromRoute] long id) => base.Delete(id);

    /// <summary>
    /// Encerra uma vaga por ID, marcando-a como inativa e removendo-a dos resultados de busca, mas mantendo os dados para histórico e relatórios. 
    /// Apenas para perfis de recrutamento.
    /// </summary>
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
