using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Admin.Users.Commands;
using EmpregaNet.Application.Admin.Users.Queries;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public AdminController(IMemoryService cacheService) : base(cacheService)
    {
    }

    /// <summary>Lista usuários. isDeleted omitido = todos; false = ativos; true = somente excluídos.</summary>
    public override async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 100,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] bool? isActive = null)
    {
        _ = isActive;

        var cacheKey = ApplicationCacheKeys.Users.AdminList(page, size, orderBy, isDeleted);
        var cached = await _cacheService.GetValueAsync<ListDataPagination<UserViewModel>>(cacheKey);
        if (cached is not null) return Ok(cached);

        var result = await _mediator.Send(new GetAllUsersQuery(page, size, orderBy, isDeleted));
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <summary>Obtém o detalhe de um utilizador pelo identificador (visão administrativa).</summary>
    public override async Task<IActionResult> GetById([FromRoute] long id)
    {
        var cacheKey = ApplicationCacheKeys.Users.AdminById(id);
        var cached = await _cacheService.GetValueAsync<UserViewModel>(cacheKey);
        if (cached is not null) return Ok(cached);

        var result = await _mediator.Send(new GetUserByIdForAdminQuery(id));
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <inheritdoc />
    /// <remarks>POST não é necessário para recurso.</remarks>
    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<IActionResult> Create([FromBody] AdminUsersCreateNotSupportedCommand entity)
        => Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));

    protected override async Task InvalidateCacheForEntity(long id = default)
    {
        await base.InvalidateCacheForEntity(id);
        await _cacheService.RemoveByPatternAsync(ApplicationCacheKeys.Users.AdminPrefix);
        if (id == default) return;

        _cacheService.Remove(ApplicationCacheKeys.Users.Me(id));
        _cacheService.Remove(ApplicationCacheKeys.Candidates.GetById(id));
    }
}
