using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Application.Auth.Events;

/// <summary>
/// Traduz <see cref="UserRegistered"/> no e-mail com o link de confirmação de conta.
/// </summary>
public sealed class UserRegisteredEmailHandler : INotificationHandler<UserRegistered>
{
    private readonly UserManager<User> _userManager;
    private readonly IAccountEmailService _accountEmail;
    private readonly AppUrlsOptions _urls;
    private readonly ILogger<UserRegisteredEmailHandler> _logger;

    public UserRegisteredEmailHandler(
        UserManager<User> userManager,
        IAccountEmailService accountEmail,
        IOptions<AppUrlsOptions> urls,
        ILogger<UserRegisteredEmailHandler> logger)
    {
        _userManager = userManager;
        _accountEmail = accountEmail;
        _urls = urls.Value;
        _logger = logger;
    }

    public async Task Handle(UserRegistered notification, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(notification.UserId.ToString());

            if (user is null)
            {
                _logger.LogWarning(
                    "Confirmação de conta {UserId} ignorada: registo não encontrado após o commit.",
                    notification.UserId);
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning(
                    "Confirmação de conta {UserId} ignorada: conta sem e-mail.",
                    notification.UserId);
                return;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = BuildConfirmationLink(user.Id, token);

            await _accountEmail.SendEmailConfirmationLinkAsync(user.Email, link, cancellationToken);

            _logger.LogInformation("Link de confirmação enviado para a conta {UserId}.", notification.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha ao enviar a confirmação de e-mail da conta {UserId}. O registo está gravado; o utilizador pode pedir o reenvio.",
                notification.UserId);
        }
    }

    private string BuildConfirmationLink(long userId, string token)
    {
        var baseUrl = _urls.PublicAppBaseUrl.TrimEnd('/');
        var path = _urls.EmailConfirmationPath.StartsWith('/') ? _urls.EmailConfirmationPath : "/" + _urls.EmailConfirmationPath;
        return $"{baseUrl}{path}?userId={userId}&token={Uri.EscapeDataString(token)}";
    }
}
