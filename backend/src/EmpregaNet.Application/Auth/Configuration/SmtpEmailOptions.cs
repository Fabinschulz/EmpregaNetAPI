namespace EmpregaNet.Application.Auth.Configuration;

/// <summary>
/// Envio transacional via SMTP (MailKit). Credenciais nunca devem ir para o repositório: use variáveis de ambiente (<c>SMTP__*</c>) ou secrets.
/// </summary>
public sealed class SmtpEmailOptions
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// <c>true</c> usa servidor SMTP real; <c>false</c> usa no-op (útil em desenvolvimento local sem caixa de correio).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Servidor SMTP do fornecedor (ex.: <c>smtp.sendgrid.net</c>, <c>smtp.mailgun.org</c>, <c>email-smtp.sa-east-1.amazonaws.com</c>).
    /// É o "endereço" da máquina à qual a API liga para entregar o e-mail.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Porta TCP: em geral <c>587</c> (STARTTLS) ou <c>465</c> (SSL implícito).</summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// <c>StartTls</c> — ligação começa em texto e negocia TLS (típico na porta 587).<br/>
    /// <c>SslOnConnect</c> — TLS desde o primeiro byte (típico na 465).<br/>
    /// <c>None</c> — sem encriptação (evitar em produção; só redes fechadas/lab).
    /// </summary>
    public string Security { get; set; } = "StartTls";

    /// <summary>Utilizador SMTP (muitas vezes igual ao FromEmail ou um API user do fornecedor).</summary>
    public string? UserName { get; set; }

    /// <summary>Palavra-passe ou chave de API SMTP (definir só por ambiente / secret).</summary>
    public string? Password { get; set; }

    /// <summary>
    /// Endereço que aparece como remetente (<c>From:</c>). Deve ser um domínio/endereço que o servidor SMTP está autorizado a enviar
    /// (verificação SPF/DKIM no painel do fornecedor). Ex.: <c>noreply@seudominio.com</c>.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>Nome amigável do remetente (ex.: "EmpregaNet"). Aparece junto ao From no cliente de e-mail.</summary>
    public string FromName { get; set; } = "EmpregaNet";
}
