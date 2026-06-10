using System.Net;

namespace EmpregaNet.Infra.Email;

/// <summary>
/// Templates HTML para e-mails transacionais (confirmação de e-mail, redefinição de senha).
/// Layout table-based com estilos inline para compatibilidade com clientes de e-mail.
/// </summary>
internal static class EmpregaNetEmailTemplates
{
    private const string ProductName = "EmpregaUAI";

    public static (string Subject, string HtmlBody) PasswordReset(string resetLink)
    {
        var safeLink = WebUtility.HtmlEncode(resetLink);
        return Build(
            subjectSuffix: "Redefinição de senha",
            preheader: "Redefina a sua senha na EmpregaUAI.",
            headline: "Redefinir senha",
            body: $"Recebemos um pedido para redefinir a senha da sua conta em <strong>{ProductName}</strong>. " +
                  "Clique no botão abaixo para escolher uma nova senha. O link expira em breve por segurança.",
            ctaLabel: "Redefinir senha",
            actionLink: safeLink,
            accent: "#1c1c1e",
            accentSoft: "#f2f2f4",
            iconGlyph: "&#128274;",
            footerHint: "Se não solicitou esta alteração, ignore este e-mail — a sua senha permanece inalterada.");
    }

    public static (string Subject, string HtmlBody) EmailConfirmation(string confirmationLink)
    {
        var safeLink = WebUtility.HtmlEncode(confirmationLink);
        return Build(
            subjectSuffix: "Confirme o seu e-mail",
            preheader: "Ative a sua conta EmpregaUAI com um clique.",
            headline: "Confirme o seu e-mail",
            body: $"Obrigado por se registar em <strong>{ProductName}</strong>. " +
                  "Confirme o endereço de e-mail para ativar a conta e começar a usar a plataforma.",
            ctaLabel: "Confirmar e-mail",
            actionLink: safeLink,
            accent: "#16a34a",
            accentSoft: "#ecfdf3",
            iconGlyph: "&#9993;",
            footerHint: $"Se não criou uma conta em {ProductName}, pode ignorar este e-mail com segurança.");
    }

    private static (string Subject, string HtmlBody) Build(
        string subjectSuffix,
        string preheader,
        string headline,
        string body,
        string ctaLabel,
        string actionLink,
        string accent,
        string accentSoft,
        string iconGlyph,
        string footerHint)
    {
        var subject = $"{ProductName} — {subjectSuffix}";
        var year = DateTime.UtcNow.Year;

        var html = $"""
            <!DOCTYPE html>
            <html lang="pt-BR" xmlns="http://www.w3.org/1999/xhtml">
            <head>
              <meta charset="utf-8"/>
              <meta name="viewport" content="width=device-width,initial-scale=1"/>
              <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
              <meta name="x-apple-disable-message-reformatting"/>
              <meta name="color-scheme" content="light"/>
              <meta name="supported-color-schemes" content="light"/>
              <title>{WebUtility.HtmlEncode(subject)}</title>
              <!--[if mso]>
              <noscript>
                <xml>
                  <o:OfficeDocumentSettings>
                    <o:PixelsPerInch>96</o:PixelsPerInch>
                  </o:OfficeDocumentSettings>
                </xml>
              </noscript>
              <![endif]-->
            </head>
            <body style="margin:0;padding:0;background-color:#f5f5f7;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:#1c1c1e;-webkit-font-smoothing:antialiased;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;mso-hide:all;">{WebUtility.HtmlEncode(preheader)}</div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f5f5f7;">
                <tr>
                  <td align="center" style="padding:32px 16px;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:520px;">
                      <tr>
                        <td style="padding:0 4px 20px 4px;text-align:center;">
                          <span style="display:inline-block;font-size:13px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;color:#636366;">{ProductName}</span>
                        </td>
                      </tr>
                      <tr>
                        <td style="background-color:#ffffff;border:1px solid rgba(0,0,0,0.08);border-radius:16px;overflow:hidden;box-shadow:0 8px 24px rgba(0,0,0,0.06);">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                            <tr>
                              <td style="height:4px;background-color:{accent};font-size:0;line-height:0;">&nbsp;</td>
                            </tr>
                            <tr>
                              <td style="padding:36px 32px 8px 32px;text-align:center;">
                                <div style="display:inline-block;width:56px;height:56px;line-height:56px;border-radius:16px;background-color:{accentSoft};font-size:26px;text-align:center;">
                                  {iconGlyph}
                                </div>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:16px 32px 0 32px;text-align:center;">
                                <h1 style="margin:0;font-size:22px;font-weight:700;line-height:1.3;color:#1c1c1e;">{WebUtility.HtmlEncode(headline)}</h1>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:12px 32px 0 32px;text-align:center;">
                                <p style="margin:0;font-size:15px;line-height:1.65;color:#636366;">{body}</p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:28px 32px 8px 32px;text-align:center;">
                                <table role="presentation" cellspacing="0" cellpadding="0" border="0" align="center">
                                  <tr>
                                    <td style="border-radius:12px;background-color:{accent};">
                                      <a href="{actionLink}" target="_blank" style="display:inline-block;padding:14px 28px;font-size:15px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:12px;mso-padding-alt:0;">{WebUtility.HtmlEncode(ctaLabel)}</a>
                                    </td>
                                  </tr>
                                </table>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:20px 32px 32px 32px;">
                                <p style="margin:0 0 10px;font-size:12px;line-height:1.5;color:#8e8e93;text-align:center;">Se o botão não funcionar, copie e cole este endereço no navegador:</p>
                                <p style="margin:0;padding:12px 14px;background-color:#f5f5f7;border:1px solid rgba(0,0,0,0.06);border-radius:10px;font-size:11px;line-height:1.55;word-break:break-all;color:#3a3a3c;font-family:Consolas,'Courier New',monospace;">{actionLink}</p>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:24px 8px 0 8px;text-align:center;">
                          <p style="margin:0 0 8px;font-size:12px;line-height:1.55;color:#8e8e93;">{WebUtility.HtmlEncode(footerHint)}</p>
                          <p style="margin:0;font-size:11px;line-height:1.5;color:#aeaeb2;">&copy; {year} {ProductName}. Plataforma de emprego.</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        return (subject, html);
    }
}
