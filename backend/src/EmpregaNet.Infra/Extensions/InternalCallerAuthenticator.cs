using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EmpregaNet.Infra.Extensions;

/// <summary>
/// Reconhece o servidor Next.js (SSR) pelo que ele <b>sabe</b>, e não pelo endereço de onde ele vem.
/// </summary>
/// <remarks>
/// Os IPs das tarefas Fargate mudam a cada início e o tráfego do SSR passa pelo ALB, então uma lista de IPs
/// (<see cref="RateLimit.BypassIps"/>) não serve. O SSR envia um segredo compartilhado no cabeçalho; quem o
/// apresenta usa um balde próprio, mais largo, porém <b>limitado</b>. Segredo errado ou ausente não dá
/// privilégio nenhum nem penalidade: a requisição segue como a de qualquer cliente.
/// </remarks>
internal sealed class InternalCallerAuthenticator
{
    /// <summary>Menor tamanho aceito para o segredo; abaixo disso ele seria adivinhável.</summary>
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

        // Compara o hash, em tempo constante, para não vazar o segredo pelo tempo de resposta.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(presented.Trim()));
        return CryptographicOperations.FixedTimeEquals(hash, _secretHash);
    }
}
