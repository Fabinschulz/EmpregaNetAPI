using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Abstraction;
using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Application.Users.Commands;

public sealed record ResendEmailConfirmationCommand(string Email) : IRequest<ResendEmailConfirmationResponse>;

public sealed record ResendEmailConfirmationResponse(string Message);

public sealed class ResendEmailConfirmationHandler : IRequestHandler<ResendEmailConfirmationCommand, ResendEmailConfirmationResponse>
{
    private const string PublicMessage =
        "Se existir uma conta pendente de confirmação para este e-mail, enviaremos um novo link.";

    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmail;
    private readonly IEmailThrottleService _emailThrottle;
    private readonly AppUrlsOptions _urls;
    private readonly ILogger<ResendEmailConfirmationHandler> _logger;

    public ResendEmailConfirmationHandler(
        UserManager<User> userManager,
        IAccountEmailService accountEmail,
        IEmailThrottleService emailThrottle,
        IOptions<AppUrlsOptions> urlsOptions,
        ILogger<ResendEmailConfirmationHandler> logger)
    {
        _userManager = userManager;
        _accountEmail = accountEmail;
        _emailThrottle = emailThrottle;
        _urls = urlsOptions.Value;
        _logger = logger;
    }

    public async Task<ResendEmailConfirmationResponse> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted || string.IsNullOrEmpty(user.Email) || user.EmailConfirmed)
        {
            _logger.LogDebug("Reenvio de confirmação omitido (sem conta pendente ou já confirmado).");
            return new ResendEmailConfirmationResponse(PublicMessage);
        }

        // Teto diário por destinatário: quando excedido, omite o envio mantendo a resposta uniforme.
        if (!await _emailThrottle.TryAcquireAsync(user.Email, cancellationToken))
        {
            _logger.LogWarning("Limite diário de e-mails atingido para o usuário {UserId}; reenvio omitido.", user.Id);
            return new ResendEmailConfirmationResponse(PublicMessage);
        }

        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var safeToken = Uri.EscapeDataString(token);
            var baseUrl = _urls.PublicAppBaseUrl.TrimEnd('/');
            var path = _urls.EmailConfirmationPath.StartsWith('/') ? _urls.EmailConfirmationPath : "/" + _urls.EmailConfirmationPath;
            var link = $"{baseUrl}{path}?userId={user.Id}&token={safeToken}";
            await _accountEmail.SendEmailConfirmationLinkAsync(user.Email, link, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao reenviar confirmação de e-mail.");
        }

        return new ResendEmailConfirmationResponse(PublicMessage);
    }
}
