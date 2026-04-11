using System.Net;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Templates HTML para e-mails transacionais (redefinição de senha, etc.).
/// </summary>
internal static class EmpregaNetEmailTemplates
{
    private const string ProductName = "EmpregaNet";
    private const string SupportHint = "Se não solicitou esta alteração, ignore este e-mail.";

    public static (string Subject, string HtmlBody) PasswordReset(string resetLink)
    {
        var safeLink = WebUtility.HtmlEncode(resetLink);
        var subject = $"{ProductName} — Redefinição de senha";

        var html = $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/></head>
            <body style="margin:0;padding:24px;background:#f4f4f5;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#1a1a1a;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:8px;padding:32px 28px;box-shadow:0 1px 3px rgba(0,0,0,.08);">
                <tr><td>
                  <p style="margin:0 0 8px;font-size:20px;font-weight:600;">{ProductName}</p>
                  <p style="margin:0 0 20px;font-size:15px;line-height:1.5;">Recebemos um pedido para redefinir a sua senha na plataforma de emprego <strong>{ProductName}</strong>.</p>
                  <p style="margin:0 0 24px;">
                    <a href="{safeLink}" style="display:inline-block;padding:12px 20px;background:#2563eb;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px;">Redefinir senha</a>
                  </p>
                  <p style="margin:0 0 12px;font-size:13px;color:#525252;line-height:1.5;">Se o botão não funcionar, copie e cole este endereço no navegador:</p>
                  <p style="margin:0 0 24px;font-size:12px;word-break:break-all;color:#2563eb;">{safeLink}</p>
                  <p style="margin:0;font-size:12px;color:#737373;">{SupportHint}</p>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        return (subject, html);
    }
}
