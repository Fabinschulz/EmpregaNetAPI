using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Interfaces;
using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Application.Users.Commands.Password;

public sealed record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

/// <summary>Resposta uniforme (não revela se o e-mail existe).</summary>
public sealed record ForgotPasswordResult(string Message);

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private const string PublicMessage =
        "Se existir uma conta para este e-mail, enviaremos instruções para redefinir a senha.";

    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmail;
    private readonly AppUrlsOptions _urls;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        UserManager<User> userManager,
        IAccountEmailService accountEmail,
        IOptions<AppUrlsOptions> urlsOptions,
        ILogger<ForgotPasswordHandler> logger)
    {
        _userManager = userManager;
        _accountEmail = accountEmail;
        _urls = urlsOptions.Value;
        _logger = logger;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted || string.IsNullOrEmpty(user.Email))
        {
            _logger.LogDebug("Pedido de reset para e-mail sem conta ativa (omitido por segurança).");
            return new ForgotPasswordResult(PublicMessage);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var safeToken = Uri.EscapeDataString(token);
        var baseUrl = _urls.PublicAppBaseUrl.TrimEnd('/');
        var path = _urls.PasswordResetPath.StartsWith('/') ? _urls.PasswordResetPath : "/" + _urls.PasswordResetPath;
        var link = $"{baseUrl}{path}?userId={user.Id}&token={safeToken}";

        await _accountEmail.SendPasswordResetLinkAsync(user.Email, link, cancellationToken);
        _logger.LogInformation("Link de redefinição de senha enviado para usuário {UserId}.", user.Id);

        return new ForgotPasswordResult(PublicMessage);
    }
}
