using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Admin.Users.Commands;
using EmpregaNet.Application.Admin.Users.Queries;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Api.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EmpregaNet.Api.Controllers.Admin;

/// <summary>
/// Marcador para <see cref="MainController{TCreateCommand,TUpdateCommand,TViewModel}"/>:
/// criação de utilizadores não é exposta aqui (use registo em <c>/api/users</c>).
/// </summary>
public sealed record AdminUsersCreateNotSupportedCommand;

/// <summary>
/// Gestão de usuários pelo administrador (listagem, detalhe, atualização do tipo, exclusão lógica).
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = Constants.AuthPolicies.Administrador)]
public class AdminController : MainController<AdminUsersCreateNotSupportedCommand, UpdateAdminUserCommand, UserViewModel>
{
    public AdminController(IOutputCacheManager cacheService) : base(cacheService)
    {
    }

    /// <summary>Lista usuários. isDeleted omitido = todos; false = ativos; true = somente excluídos.</summary>
    [HttpGet]
    [OutputCache(PolicyName = OutputCachePolicies.AuthenticatedRead, Tags = [ApplicationCacheTags.AdminUsers])]
    public override async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null)
    {
        _ = isActive;
        _ = search;
        var result = await _mediator.Send(new GetAllUsersQuery(page, size, orderBy, isDeleted));
        return Ok(result);
    }

    /// <summary>Obtém o detalhe de um utilizador pelo identificador (visão administrativa).</summary>
    [HttpGet("{id:long}")]
    [OutputCache(PolicyName = OutputCachePolicies.AuthenticatedRead, Tags = [ApplicationCacheTags.AdminUsers])]
    public override async Task<IActionResult> GetById([FromRoute] long id)
    {
        var result = await _mediator.Send(new GetUserByIdForAdminQuery(id));
        return Ok(result);
    }

    /// <inheritdoc />
    /// <remarks>POST não é necessário para recurso.</remarks>
    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<IActionResult> Create([FromBody] AdminUsersCreateNotSupportedCommand entity)
        => Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));

    protected override Task InvalidateCacheForEntity(long id = default)
        => _outputCache.InvalidateAdminUsersAsync(id);
}
