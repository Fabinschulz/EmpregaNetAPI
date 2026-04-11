using EmpregaNet.Application.Auth.Configuration;
using EmpregaNet.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace EmpregaNet.Infra.Security;

public sealed class GoogleIdTokenValidator : IGoogleIdTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleIdTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (_options.ClientIds is null || _options.ClientIds.Length == 0)
            return null;

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _options.ClientIds.ToList()
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (string.IsNullOrEmpty(payload.Email) || string.IsNullOrEmpty(payload.Subject))
                return null;

            return new GoogleIdTokenPayload(payload.Subject, payload.Email, payload.EmailVerified);
        }
        catch
        {
            return null;
        }
    }
}
