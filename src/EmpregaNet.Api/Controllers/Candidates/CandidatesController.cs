using EmpregaNet.Application.Utils;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Candidates;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Constants.AuthPolicies.Recrutamento)]
public class CandidatesController : ControllerBase
{
    private IMediator _iMediator = null!;
    private IMediator Mediator => _iMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();
    private readonly IMemoryService _cacheService;

    public CandidatesController(IMemoryService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListDataPagination<UserViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string? orderBy = null)
    {
        var cacheKey = ApplicationCacheKeys.Candidates.GetAll(page, size, orderBy);
        var cached = await _cacheService.GetValueAsync<ListDataPagination<UserViewModel>>(cacheKey);
        if (cached is not null) return Ok(cached);

        var result = await Mediator.Send(new GetAllCandidatesQuery(page, size, orderBy));
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var cacheKey = ApplicationCacheKeys.Candidates.GetById(id);
        var cached = await _cacheService.GetValueAsync<UserViewModel>(cacheKey);
        if (cached is not null) return Ok(cached);

        var result = await Mediator.Send(new GetCandidateByIdQuery(id));
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }
}
