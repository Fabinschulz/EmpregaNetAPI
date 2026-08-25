using EmpregaNet.Api.Configuration;
using EmpregaNet.Application.Dashboard.Queries;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EmpregaNet.Api.Controllers.Dashboard;

/// <summary>
/// Métricas e analytics da operação de recrutamento.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Constants.AuthPolicies.Recrutamento)]
[ApiExplorerSettings(GroupName = Constants.OpenApi.V1)]
public class DashboardController : ControllerBase
{
    private IMediator _mediator => HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Indicadores principais e funil de recrutamento do período, com comparação contra o período
    /// anterior de igual duração.
    /// </summary>
    [HttpGet("overview")]
    [OutputCache(PolicyName = OutputCachePolicies.DashboardRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardOverviewViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetOverview([FromQuery] DashboardRequest request)
    {
        var result = await _mediator.Send(new GetDashboardOverviewQuery(request.ToFilter()));
        return Ok(result);
    }

    /// <summary>
    /// Séries temporais do período: candidaturas, novos candidatos e vagas publicadas.
    /// </summary>
    [HttpGet("trends")]
    [OutputCache(PolicyName = OutputCachePolicies.DashboardRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardTrendsViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetTrends([FromQuery] DashboardTrendsRequest request)
    {
        var result = await _mediator.Send(new GetDashboardTrendsQuery(request.ToFilter(), request.Granularity));
        return Ok(result);
    }

    /// <summary>
    /// Distribuições do período: candidaturas por status, e concentração por área profissional pelos
    /// dois lados (candidaturas e vagas publicadas).
    /// </summary>
    [HttpGet("distribution")]
    [OutputCache(PolicyName = OutputCachePolicies.DashboardRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardDistributionViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetDistribution([FromQuery] DashboardRequest request)
    {
        var result = await _mediator.Send(new GetDashboardDistributionQuery(request.ToFilter()));
        return Ok(result);
    }

    /// <summary>
    /// Ranking de desempenho das vagas, com a média de candidaturas por vaga como base de comparação.
    /// </summary>
    [HttpGet("jobs")]
    [OutputCache(PolicyName = OutputCachePolicies.DashboardRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardJobsViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetJobs([FromQuery] DashboardJobsRequest request)
    {
        var result = await _mediator.Send(
            new GetDashboardJobsQuery(request.ToFilter(), request.Ranking, request.Limit, request.OnlyActive));
        return Ok(result);
    }

    /// <summary>
    /// Leituras derivadas do período: vagas estagnadas, concentração por área e desvio da vaga líder
    /// sobre a média. Devolve lista vazia quando os dados não sustentam nenhuma.
    /// </summary>
    [HttpGet("insights")]
    [OutputCache(PolicyName = OutputCachePolicies.DashboardRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardInsightsViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
    public async Task<IActionResult> GetInsights([FromQuery] DashboardRequest request)
    {
        var result = await _mediator.Send(new GetDashboardInsightsQuery(request.ToFilter()));
        return Ok(result);
    }
}
