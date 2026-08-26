using EmpregaNet.Api.Configuration;
using EmpregaNet.Application.Auth.Commands;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Utils;
using EmpregaNet.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Auth;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private IMediator _iMediator = null!;
    private IMediator Mediator => _iMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();
    private readonly AuthCookieService _authCookies;

    public AuthController(AuthCookieService authCookies)
    {
        _authCookies = authCookies;
    }

    /// <summary>Registra um novo usuário com nome de usuário, e-mail, CPF e senha.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(DomainError))]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var id = await Mediator.Send(command);
        return Created($"api/users/{id}", "Enviámos um e-mail de confirmação para o endereço indicado. Confirme o seu e-mail para ativar a conta.");
    }

    /// <summary>Autentica um usuário e retorna um token JWT para acesso aos recursos protegidos.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserLoggedViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await Mediator.Send(command);
        _authCookies.AppendLoginCookies(Response, result);
        return Ok(result);
    }

    /// <summary>Renova o access token com um refresh token válido (rotação: o refresh antigo deixa de ser válido).</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(UserLoggedViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand? command)
    {
        var refresh = ResolveRefreshToken(command?.RefreshToken);
        var result = await Mediator.Send(new RefreshTokenCommand(refresh));
        _authCookies.AppendLoginCookies(Response, result);
        return Ok(result);
    }

    /// <summary>Encerra a sessão: revoga o refresh token atual e limpa os cookies de autenticação.</summary>
    /// <remarks>
    /// Anónimo por design para funcionar mesmo com o access token expirado; a credencial revogada é o refresh
    /// token (lido do cookie httpOnly ou do corpo). Idempotente, devolve 200 mesmo sem token a revogar.
    /// </remarks>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand? command)
    {
        var refresh = ResolveRefreshToken(command?.RefreshToken);

        await Mediator.Send(new LogoutCommand(refresh));
        _authCookies.ClearLoginCookies(Response);
        return Ok(new { message = "Sessão encerrada com sucesso." });
    }

    /// <summary>
    /// Resolve o refresh token: corpo da requisição primeiro, cookie httpOnly como fallback.
    /// </summary>
    /// <remarks>
    /// Usa verificação de vazio em vez de <c>??</c>: um corpo com <c>{"refreshToken": ""}</c>
    /// produz string vazia (não nula), então o <c>??</c> não caía para o cookie e a renovação
    /// falhava mesmo havendo um cookie válido.
    /// </remarks>
    private string ResolveRefreshToken(string? fromBody)
    {
        if (!string.IsNullOrWhiteSpace(fromBody))
            return fromBody;

        var fromCookie = Request.Cookies[Constants.AuthCookies.RefreshToken];

        return string.IsNullOrWhiteSpace(fromCookie) ? string.Empty : fromCookie;
    }

    /// <summary>Autentica com Google: envie o <c>id_token</c> obtido no cliente (Sign-In).</summary>
    [HttpPost("login/google")]
    [ProducesResponseType(typeof(UserLoggedViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> LoginWithGoogle([FromBody] LoginWithGoogleCommand command)
    {
        var result = await Mediator.Send(command);
        _authCookies.AppendLoginCookies(Response, result);
        return Ok(result);
    }

    /// <summary>Solicita e-mail com link para redefinir senha.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Confirma o e-mail com <c>userId</c> e <c>token</c> do link enviado após o registo.</summary>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "E-mail confirmado com sucesso. Já pode iniciar sessão." });
    }

    /// <summary>Reenvia o link de confirmação de e-mail.</summary>
    [HttpPost("resend-email-confirmation")]
    [ProducesResponseType(typeof(ResendEmailConfirmationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Redefine a senha com o token recebido por e-mail (query string do link).</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "Senha redefinida com sucesso." });
    }
}
