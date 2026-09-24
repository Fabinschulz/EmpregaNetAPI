using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.JobApplications.Queries;

/// <summary>
/// Interpreta o filtro <c>status</c> das listagens de candidaturas do recrutamento.
/// </summary>
/// <remarks>
/// Fonte única para os dois endpoints (<c>GET /api/jobapplications</c> e
/// <c>GET /api/jobapplications/job/{jobId}</c>): o mesmo valor tem de ser aceito ou recusado igual nos
/// dois, com o mesmo erro.
/// </remarks>
public static class ApplicationStatusParser
{
    /// <summary>
    /// Vazio ou ausente = sem filtro (<c>null</c>). Só o nome do enum, sem distinção de caixa; número,
    /// lista com vírgula, valor desconhecido ou <see cref="ApplicationStatusEnum.NaoSelecionado"/> (não é
    /// um status real) são recusados com <see cref="DomainErrorEnum.INVALID_QUERY_FILTER"/>.
    /// </summary>
    /// <remarks>
    /// Fonte única da validação de status nos dois endpoints: os validators do FluentValidation não
    /// têm regra de <c>Status</c>, para que todo valor inválido saia com o mesmo código de erro.
    /// </remarks>
    public static ApplicationStatusEnum? ParseOrNull(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;

        if (!EnumNameParser.TryParseName<ApplicationStatusEnum>(status, out var parsed) ||
            parsed == ApplicationStatusEnum.NaoSelecionado)
        {
            throw new ValidationAppException(
                nameof(status),
                "Status de candidatura inválido para filtro.",
                DomainErrorEnum.INVALID_QUERY_FILTER);
        }

        return parsed;
    }
}
