using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Utils;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Api.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EmpregaNet.Api.Controllers.Candidates;

/// <summary>
/// Consulta de candidatos (utilizadores com perfil de candidato) para equipe de recrutamento.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
public class CandidatesController : ControllerBase
{
    private IMediator _iMediator = null!;
    private IMediator Mediator => _iMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>Lista candidatos com paginação e ordenação.</summary>
    [HttpGet]
    [OutputCache(PolicyName = OutputCachePolicies.AuthenticatedRead, Tags = [ApplicationCacheTags.Candidates])]
    [ProducesResponseType(typeof(ListDataPagination<UserViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string? orderBy = null)
    {
        var result = await Mediator.Send(new GetAllCandidatesQuery(page, size, orderBy));
        return Ok(result);
    }

    /// <summary>Obtém o detalhe de um candidato pelo identificador.</summary>
    [HttpGet("{id:long}")]
    [OutputCache(PolicyName = OutputCachePolicies.AuthenticatedRead, Tags = [ApplicationCacheTags.Candidates])]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var result = await Mediator.Send(new GetCandidateByIdQuery(id));
        return Ok(result);
    }
}
