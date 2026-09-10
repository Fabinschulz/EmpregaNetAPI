using System.Net;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.JobApplications.Events;

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
            footerHint: "Se não solicitou esta alteração, ignore este e-mail - a sua senha permanece inalterada.");
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

    /// <summary>
    /// Notificação de alteração do estado da candidatura: o assunto, o tom visual e o corpo variam
    /// conforme a razão do aviso e o novo estado; o conteúdo central (vaga, empresa, estado e data)
    /// permanece igual em todos os cenários.
    /// </summary>
    public static (string Subject, string HtmlBody) JobApplicationStatus(
        string candidateName,
        string jobTitle,
        string companyName,
        string statusDescription,
        ApplicationStatusEnum newStatus,
        string occurredAt,
        string applicationsLink,
        JobApplicationNotificationReason reason)
    {
        var safeLink = WebUtility.HtmlEncode(applicationsLink);
        var safeJob = WebUtility.HtmlEncode(jobTitle);
        var safeCompany = WebUtility.HtmlEncode(companyName);
        var safeStatus = WebUtility.HtmlEncode(statusDescription);
        var safeDate = WebUtility.HtmlEncode(occurredAt);

        var (subjectSuffix, headline, intro, accent, accentSoft, iconGlyph) = reason switch
        {
            JobApplicationNotificationReason.Applied => (
                $"Candidatura enviada - {jobTitle}",
                "Candidatura recebida",
                $"Recebemos a sua candidatura para <strong>{safeJob}</strong>. Ela já está registada e você acompanha cada mudança por aqui.",
                "#2563eb", "#eff6ff", "&#128221;"),

            JobApplicationNotificationReason.JobClosed => (
                $"Vaga encerrada - {jobTitle}",
                "A vaga foi encerrada",
                $"A empresa encerrou a vaga <strong>{safeJob}</strong>, e por isso a sua candidatura foi finalizada. Continuam disponíveis outras vagas na plataforma.",
                "#d97706", "#fffbeb", "&#128683;"),

            JobApplicationNotificationReason.CanceledByCandidate => (
                $"Candidatura cancelada - {jobTitle}",
                "Candidatura cancelada",
                $"Confirmamos o cancelamento da sua candidatura para <strong>{safeJob}</strong>. Se mudar de ideia e a vaga continuar aberta, pode candidatar-se de novo.",
                "#6b7280", "#f3f4f6", "&#10006;"),

            _ => StatusChangeTreatment(newStatus, jobTitle, safeJob, safeStatus)
        };

        var greeting = string.IsNullOrWhiteSpace(candidateName)
            ? string.Empty
            : $"Olá, {WebUtility.HtmlEncode(candidateName.Trim())}! ";

        var details = $"""
            {greeting}{intro}
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:20px 0 0;text-align:left;">
              <tr>
                <td style="padding:6px 0;font-size:13px;color:#8e8e93;width:38%;">Vaga</td>
                <td style="padding:6px 0;font-size:14px;color:#1c1c1e;font-weight:600;">{safeJob}</td>
              </tr>
              <tr>
                <td style="padding:6px 0;font-size:13px;color:#8e8e93;">Empresa</td>
                <td style="padding:6px 0;font-size:14px;color:#1c1c1e;">{safeCompany}</td>
              </tr>
              <tr>
                <td style="padding:6px 0;font-size:13px;color:#8e8e93;">Situação</td>
                <td style="padding:6px 0;font-size:14px;color:#1c1c1e;font-weight:600;">{safeStatus}</td>
              </tr>
              <tr>
                <td style="padding:6px 0;font-size:13px;color:#8e8e93;">Atualizado em</td>
                <td style="padding:6px 0;font-size:14px;color:#1c1c1e;">{safeDate}</td>
              </tr>
            </table>
            """;

        return Build(
            subjectSuffix: subjectSuffix,
            preheader: $"{statusDescription} - {jobTitle}",
            headline: headline,
            body: details,
            ctaLabel: "Ver minhas candidaturas",
            actionLink: safeLink,
            accent: accent,
            accentSoft: accentSoft,
            iconGlyph: iconGlyph,
            footerHint: "Você recebe este aviso porque tem uma candidatura ativa na EmpregaUAI.");
    }
    
    private static (string SubjectSuffix, string Headline, string Intro, string Accent, string AccentSoft, string IconGlyph)
        StatusChangeTreatment(ApplicationStatusEnum newStatus, string jobTitle, string safeJob, string safeStatus)
    {
        return newStatus switch
        {
            ApplicationStatusEnum.Approved => (
                $"Você avançou no processo - {jobTitle}",
                "Sua candidatura foi aprovada",
                $"Boa notícia: a sua candidatura para <strong>{safeJob}</strong> foi <strong>aprovada</strong>. A empresa entrará em contato com os próximos passos.",
                "#16a34a", "#ecfdf3", "&#127881;"),

            ApplicationStatusEnum.Rejected => (
                $"Atualização da candidatura - {jobTitle}",
                "Sua candidatura não seguiu no processo",
                $"A empresa concluiu a avaliação da sua candidatura para <strong>{safeJob}</strong> e decidiu não seguir com ela desta vez. Isto não impede novas candidaturas a outras vagas.",
                "#dc2626", "#fef2f2", "&#128533;"),

            ApplicationStatusEnum.Processing => (
                $"Sua candidatura está em análise - {jobTitle}",
                "Sua candidatura está em análise",
                $"A empresa começou a analisar a sua candidatura para <strong>{safeJob}</strong>. Avisamos aqui assim que houver uma decisão.",
                "#2563eb", "#eff6ff", "&#128269;"),

            ApplicationStatusEnum.Finished => (
                $"Processo concluído - {jobTitle}",
                "O processo foi concluído",
                $"O processo seletivo da vaga <strong>{safeJob}</strong> foi concluído. Obrigado por ter participado.",
                "#6b7280", "#f3f4f6", "&#9989;"),

            _ => (
                $"Atualização da candidatura - {jobTitle}",
                "Sua candidatura mudou de estado",
                $"Há novidade na sua candidatura para <strong>{safeJob}</strong>. O estado atual é <strong>{safeStatus}</strong>.",
                "#6b7280", "#f3f4f6", "&#128276;")
        };
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
        var subject = $"{ProductName} - {subjectSuffix}";
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
