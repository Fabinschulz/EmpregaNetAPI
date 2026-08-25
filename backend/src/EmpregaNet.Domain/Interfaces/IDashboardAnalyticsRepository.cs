using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Interfaces;

/// <summary>
/// Leituras agregadas do dashboard de métricas.
/// </summary>
public interface IDashboardAnalyticsRepository
{
    /// <summary>
    /// Contagens do período e do período anterior de igual duração: vagas criadas e encerradas,
    /// candidaturas, candidatos novos e só na visão de plataforma, utilizadores e empresas novos.
    /// </summary>
    Task<DashboardComparedCounters> GetComparedCountersAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Estado acumulado até <c>filter.Range.ToUtcExclusive</c>.
    /// </summary>
    Task<DashboardTotals> GetTotalsAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>Candidaturas por status, no período e no anterior.</summary>
    Task<IReadOnlyList<DashboardStatusComparison>> GetApplicationsByStatusAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken);

    /// <summary>Vagas publicadas por dia local no período.</summary>
    Task<IReadOnlyList<DashboardDailyPoint>> GetJobsDailySeriesAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>Candidaturas recebidas por dia local no período.</summary>
    Task<IReadOnlyList<DashboardDailyPoint>> GetApplicationsDailySeriesAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Candidatos por dia local no período: primeira candidatura de cada utilizador.
    /// </summary>
    Task<IReadOnlyList<DashboardDailyPoint>> GetNewCandidatesDailySeriesAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>Vagas publicadas no período por área profissional.</summary>
    Task<IReadOnlyList<DashboardEnumCount<JobAreaEnum>>> GetJobsByAreaAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>Candidaturas do período por área da vaga.</summary>
    Task<IReadOnlyList<DashboardEnumCount<JobAreaEnum>>> GetApplicationsByAreaAsync(DashboardFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Ranking de vagas por candidaturas no período, limitado a <paramref name="limit"/> linhas.
    /// </summary>
    /// <param name="filter">Escopo, janela e recortes já autorizados.</param>
    /// <param name="ranking">Critério de ordenação do ranking.</param>
    /// <param name="limit">Número máximo de linhas devolvidas.</param>
    /// <param name="onlyActive">
    /// <c>true</c> restringe às vagas abertas — é o recorte útil para agir. Vaga encerrada aparece
    /// no histórico, mas não há o que fazer com ela.
    /// </param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    Task<IReadOnlyList<DashboardJobPerformance>> GetJobPerformanceAsync(
        DashboardFilter filter,
        DashboardJobRankingEnum ranking,
        int limit,
        bool onlyActive,
        CancellationToken cancellationToken);

    /// <summary>
    /// Média de candidaturas por vaga publicada no período, e quantas vagas entraram nessa média.
    /// </summary>
    Task<(double Average, int JobCount)> GetAverageApplicationsPerJobAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken);

    /// <summary>
    /// Indicadores operacionais das vagas do escopo: abertas, sem candidatura, estagnadas e
    /// encerradas no período.
    /// </summary>
    Task<DashboardJobHealth> GetJobHealthAsync(
        DashboardFilter filter,
        int staleDays,
        CancellationToken cancellationToken);
}
