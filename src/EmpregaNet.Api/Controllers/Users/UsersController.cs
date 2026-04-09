using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Application.Users.Commands.Profile;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EmpregaNet.Api.Controllers.Users;

/// <summary>Registro, login e gestão da própria conta (<c>/me</c>).</summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private IMediator _iMediator = null!;
    private IMediator Mediator => _iMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();
    private readonly IMemoryService _cacheService;

    public UsersController(IMemoryService cacheService)
    {
        _cacheService = cacheService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(DomainError))]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var id = await Mediator.Send(command);
        return Created($"api/users/{id}", $"Usuário criado com sucesso. ID: {id}");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserLoggedViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue("userId")
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            var uncached = await Mediator.Send(new GetCurrentUserQuery());
            return Ok(uncached);
        }

        var cacheKey = ApplicationCacheKeys.Users.Me(userId);
        var cached = await _cacheService.GetValueAsync<UserViewModel>(cacheKey);
        if (cached is not null) return Ok(cached);

        var result = await Mediator.Send(new GetCurrentUserQuery());
        await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    /// <summary>Atualiza dados do próprio usuário (e-mail, nome de usuário, telefone).</summary>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Encerra a própria conta (exclusão lógica; o registro permanece para auditoria).</summary>
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    public async Task<IActionResult> DeleteMyAccount()
    {
        await Mediator.Send(new DeleteMyProfileCommand());
        return NoContent();
    }

}
