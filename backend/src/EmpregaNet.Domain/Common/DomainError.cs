using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common;

public class DomainError
{
    /// <summary>
    /// Código de status HTTP associado ao erro.
    /// </summary>
    public required int StatusCode { get; set; }

    /// <summary>
    /// Código de domínio do erro encontrado. Serializado pelo nome (ex.: <c>"INVALID_PARAMS"</c>).
    /// </summary>
    public DomainErrorEnum Code { get; set; }

    /// <summary>
    /// Uma frase para o utilizador ler.
    /// Se houver apenas um erro de campo, esta frase é a mensagem desse erro; caso contrário, é um título genérico.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Uma entrada por falha. Vazia quando o erro não tem detalhe a expor ao cliente.
    /// </summary>
    public IReadOnlyList<DomainErrorItem> Errors { get; set; } = [];

    /// <summary>
    /// ID de correlação de telemetria. Ajuda a encontrar os logs da requisição.
    /// </summary>
    public required string CorrelationId { get; set; }

    /// <summary>
    /// Preenchido apenas em <c>Development</c>.
    /// </summary>
    public string? StackTrace { get; set; }
}

/// <summary>
/// Uma falha individual dentro de um <see cref="DomainError"/>.
/// </summary>
public sealed class DomainErrorItem
{
    public string? Field { get; set; }
    public required string Message { get; set; }
    public DomainErrorEnum Code { get; set; }
}
