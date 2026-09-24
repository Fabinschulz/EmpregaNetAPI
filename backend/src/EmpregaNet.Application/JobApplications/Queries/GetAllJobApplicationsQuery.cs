using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;

namespace EmpregaNet.Application.JobApplications.Queries;

/// <summary>
/// Todas as candidaturas (recrutamento), no escopo de empresa de quem consulta.
/// <paramref name="Status"/> filtra pelo nome do <c>ApplicationStatusEnum</c> (case-insensitive);
/// <paramref name="Search"/> filtra por nome ou e-mail do candidato e título da vaga (case-insensitive).
/// </summary>
/// <remarks>
/// Record dedicado, e não o <c>GetAllQuery&lt;T&gt;</c> genérico: <c>Status</c> é conceito só de
/// candidatura e vazaria para as outras entidades que partilham o genérico.
/// <paramref name="IsDeleted"/> é aceito por compatibilidade com o contrato HTTP existente e não filtra:
/// a listagem mostra sempre as candidaturas não excluídas.
/// </remarks>
public sealed record GetAllJobApplicationsQuery(
    int Page, int Size, string? OrderBy, bool? IsDeleted, string? Search, string? Status)
    : IRequest<ListDataPagination<JobApplicationViewModel>>, IPaginatedQuery;
