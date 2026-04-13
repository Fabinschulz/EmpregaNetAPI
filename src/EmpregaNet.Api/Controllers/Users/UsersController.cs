using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Cache;
using EmpregaNet.Application.Users.Commands;
using EmpregaNet.Application.Users.Queries;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
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

    /// <summary>Registra um novo usuário com nome de usuário, e-mail e senha.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(DomainError))]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var id = await Mediator.Send(command);
        return Created($"api/users/{id}", $"Usuário criado com sucesso. ID: {id}. Confirme o e-mail para poder iniciar sessão.");
    }

    /// <summary>Autentica um usuário e retorna um token JWT para acesso aos recursos protegidos.</summary>
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

    /// <summary>Renova o access token com um refresh token válido (rotação: o refresh antigo deixa de ser válido).</summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(UserLoggedViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Autentica com Google: envie o <c>id_token</c> obtido no cliente (Sign-In SDK). Requer <c>GoogleAuth:ClientIds</c> configurado.</summary>
    [AllowAnonymous]
    [HttpPost("login/google")]
    [ProducesResponseType(typeof(UserLoggedViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> LoginWithGoogle([FromBody] LoginWithGoogleCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Solicita e-mail com link para redefinir senha (resposta genérica por segurança).</summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Confirma o e-mail com <c>userId</c> e <c>token</c> do link enviado após o registo.</summary>
    [AllowAnonymous]
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "E-mail confirmado com sucesso. Já pode iniciar sessão." });
    }

    /// <summary>Reenvia o link de confirmação de e-mail (resposta genérica por segurança).</summary>
    [AllowAnonymous]
    [HttpPost("resend-email-confirmation")]
    [ProducesResponseType(typeof(ResendEmailConfirmationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Redefine a senha com o token recebido por e-mail (query string do link).</summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "Senha redefinida com sucesso." });
    }

    /// <summary>
    /// Retorna os dados do usuário autenticado (ID, nome de usuário, e-mail, tipo de usuário, etc.).
    /// </summary>
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

    /// <summary>Altera a senha do utilizador autenticado (exige senha atual).</summary>
    [Authorize]
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
