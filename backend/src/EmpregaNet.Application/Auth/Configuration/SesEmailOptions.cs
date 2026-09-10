namespace EmpregaNet.Application.Auth.Configuration;

/// <summary>
/// Envio transacional via Amazon SES. Não há utilizador nem palavra-passe aqui de propósito:
/// a autenticação usa a cadeia de credenciais padrão da AWS (IAM role da instância em produção, perfil
/// ou variáveis <c>AWS_*</c> em desenvolvimento), portanto esta secção não guarda segredo nenhum.
/// </summary>
public sealed class SesEmailOptions
{
    public const string SectionName = "Ses";
    public bool Enabled { get; set; }
    public string? Region { get; set; }

    /// <summary>
    /// Endereço que aparece como remetente (<c>From:</c>). Tem de ser uma identidade verificada no SES -
    /// e-mail verificado, ou endereço num domínio verificado com DKIM. Ex.: <c>noreply@seudominio.com</c>.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "EmpregaNet";
}
