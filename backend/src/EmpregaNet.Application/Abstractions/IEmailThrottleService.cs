namespace EmpregaNet.Application.Abstraction;

/// <summary>
/// Limita o volume de e-mails transacionais por destinatário (anti-abuso/custo).
/// </summary>
public interface IEmailThrottleService
{
    /// <summary>
    /// Consome uma unidade do orçamento diário do destinatário.
    /// Devolve <c>false</c> quando o teto diário foi atingido, o chamador deve
    /// omitir o envio silenciosamente (mantendo a resposta pública uniforme).
    /// </summary>
    Task<bool> TryAcquireAsync(string email, CancellationToken cancellationToken = default);
}
