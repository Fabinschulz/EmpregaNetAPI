using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain;

public class DomainError
{
    /// <summary>
    /// Código de status HTTP associado ao erro
    /// </summary>
    public required int StatusCode { get; set; }

    /// <summary>
    /// Código de domínio do erro encontrado
    /// </summary>
    public DomainErrorEnum Code { get; set; }

    /// <summary>
    /// Mensagem amigável para ser exibida no usuário
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Detalhes adicionais do erro que podem ajudar a identificar a causa
    /// </summary>
    public required object Details { get; set; }

    /// <summary>
    /// ID de correlação de telemetria. Ajuda a encontrar logs da requisição e identificar possivel problemas.
    /// </summary>
    public required string CorrelationId { get; set; }
}

