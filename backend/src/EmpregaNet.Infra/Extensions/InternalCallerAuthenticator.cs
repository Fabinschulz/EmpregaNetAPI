using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EmpregaNet.Infra.Extensions;

/// <summary>
/// Reconhece o SSR do Next.js por um segredo no cabeçalho, já que o IP das tarefas Fargate muda a cada início.
/// Segredo errado ou ausente não dá privilégio nem penalidade: a requisição segue como a de qualquer cliente.
/// </summary>
internal sealed class InternalCallerAuthenticator
{
    public const int MinimumSecretLength = 32;

    private readonly string _headerName;
    private readonly byte[]? _secretHash;

    public InternalCallerAuthenticator(InternalCallerOptions options)
    {
        _headerName = string.IsNullOrWhiteSpace(options.HeaderName) ? "X-Internal-Caller" : options.HeaderName.Trim();

        if (!options.IsEnabled)
        {
            return;
        }

        if (options.Secret!.Trim().Length < MinimumSecretLength)
        {
            throw new InvalidOperationException(
                $"'{RateLimit.SectionName}:InternalCaller:Secret' precisa ter ao menos {MinimumSecretLength} caracteres.");
        }

        _secretHash = SHA256.HashData(Encoding.UTF8.GetBytes(options.Secret.Trim()));
    }

    public bool IsEnabled => _secretHash is not null;

    public bool IsAuthenticated(HttpRequest request)
    {
        if (_secretHash is null)
        {
            return false;
        }

        if (!request.Headers.TryGetValue(_headerName, out var values) || values.Count != 1)
        {
            return false;
        }

        var presented = values[0];
        if (string.IsNullOrEmpty(presented))
        {
            return false;
        }

        // Hash + tempo constante: não vaza o segredo pelo tempo de resposta.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(presented.Trim()));
        return CryptographicOperations.FixedTimeEquals(hash, _secretHash);
    }
}
