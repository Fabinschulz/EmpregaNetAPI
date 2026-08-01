using EmpregaNet.Api.Configuration;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace EmpregaNet.Api.Controllers.Users;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private IMediator _iMediator = null!;
    private IMediator Mediator => _iMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Retorna os dados do usuário autenticado (ID, nome de usuário, e-mail, tipo de usuário, etc.).
    /// </summary>
    [HttpGet("me")]
    [OutputCache(PolicyName = OutputCachePolicies.UserProfileRead)]
    [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
    public async Task<IActionResult> Me()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return Ok(result);
    }

    /// <summary>Atualiza dados do próprio usuário (e-mail, nome de usuário, telefone).</summary>
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

    /// <summary>
    /// Altera a senha do utilizador autenticado (exige senha atual).
    /// </summary>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "Senha alterada com sucesso." });
    }

    /// <summary>Encerra a própria conta (exclusão lógica; o registro permanece para auditoria).</summary>
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
