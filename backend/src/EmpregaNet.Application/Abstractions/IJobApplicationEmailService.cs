using EmpregaNet.Application.JobApplications.Events;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Abstraction;

/// <summary>
/// Notificação por e-mail do andamento de uma candidatura.
/// </summary>
/// <remarks>
/// Separado de <see cref="IAccountEmailService"/> de propósito: aquele trata de credencial e acesso
/// (reset de senha, confirmação de conta), este de andamento de processo selectivo. São públicos
/// diferentes, textos diferentes e decisivo políticas de teto diferentes (ver
/// <c>JobApplicationEmailService</c>).
/// </remarks>
public interface IJobApplicationEmailService
{
    /// <summary>Envia ao candidato a notificação do estado actual da candidatura.</summary>
    Task SendStatusNotificationAsync(JobApplicationEmailModel model, CancellationToken cancellationToken = default);
}

/// <summary>
/// Tudo o que o e-mail de andamento precisa de mostrar, já resolvido.
/// </summary>
/// <remarks>
/// O serviço de transporte não consulta nada: recebe o conteúdo pronto. Isso mantém a Infra sem
/// regra de negócio e torna o conteúdo do e-mail verificável por teste sem base de dados.
/// </remarks>
/// <param name="CandidateEmail">Destinatário. Vem do agregado, nunca da requisição (CA-08).</param>
/// <param name="CandidateName">Nome para a saudação; pode vir vazio se o utilizador não tiver nome.</param>
/// <param name="JobTitle">Título da vaga.</param>
/// <param name="CompanyName">Empresa que publicou a vaga.</param>
/// <param name="StatusDescription">Estado em pt-BR, como o candidato o vê na tela.</param>
/// <param name="NewStatus">Estado em si. O texto não basta para o template escolher tom: reprovada e
/// aprovada são ambas "mudança de status" e não podem sair com a mesma cor.</param>
/// <param name="OccurredAt">Momento da mudança.</param>
/// <param name="ApplicationsUrl">Link para a área autenticada de candidaturas. Sem token.</param>
/// <param name="Reason">O que provocou a notificação; escolhe assunto e corpo.</param>
public sealed record JobApplicationEmailModel(
    string CandidateEmail,
    string CandidateName,
    string JobTitle,
    string CompanyName,
    string StatusDescription,
    ApplicationStatusEnum NewStatus,
    DateTimeOffset OccurredAt,
    string ApplicationsUrl,
    JobApplicationNotificationReason Reason);
