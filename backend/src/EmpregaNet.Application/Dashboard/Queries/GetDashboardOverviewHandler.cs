using EmpregaNet.Application.Dashboard.UseCase;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Dashboard.Queries;

/// <summary>Indicadores principais e funil de recrutamento do período.</summary>
public sealed record GetDashboardOverviewQuery(DashboardFilterInput Filter) : IRequest<DashboardOverviewViewModel>;

public sealed class GetDashboardOverviewHandler
    : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewViewModel>
{
    private readonly IDashboardAnalyticsRepository _repository;
    private readonly IDashboardContextFactory _contextFactory;
    private readonly ILogger<GetDashboardOverviewHandler> _logger;

    public GetDashboardOverviewHandler(
        IDashboardAnalyticsRepository repository,
        IDashboardContextFactory contextFactory,
        ILogger<GetDashboardOverviewHandler> logger)
    {
        _repository = repository;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<DashboardOverviewViewModel> Handle(
        GetDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var context = await _contextFactory.CreateAsync(request.Filter, cancellationToken);

        _logger.LogInformation(
            "Dashboard: visão geral (Escopo: {Scope}, Período: {Period}, {From} a {To})",
            context.Scope.Scope.IsPlatformWide ? "plataforma" : $"empresa {context.Scope.Scope.CompanyId}",
            request.Filter.Period,
            context.Period.FromLocal,
            context.Period.ToLocalInclusive);

        var totals = await _repository.GetTotalsAsync(context.Filter, cancellationToken);
        var counters = await _repository.GetComparedCountersAsync(context.Filter, cancellationToken);
        var byStatus = await _repository.GetApplicationsByStatusAsync(context.Filter, cancellationToken);

        var funnel = BuildFunnel(byStatus);
        var previousConversion = ConversionRate(
            EffectiveApplications(byStatus, previousPeriod: true),
            CountOf(byStatus, ApplicationStatusEnum.Approved, previousPeriod: true)
                + CountOf(byStatus, ApplicationStatusEnum.Finished, previousPeriod: true));

        return new DashboardOverviewViewModel
        {
            Meta = _contextFactory.CreateMeta(context, request.Filter, DashboardDomainGaps.Overview),
            Kpis = BuildKpis(context, totals, counters, funnel.ConversionRate, previousConversion),
            Funnel = funnel
        };
    }

    /// <summary>
    /// Monta os indicadores da operação: cinco no escopo de empresa, seis na plataforma.
    /// </summary>
    private static IReadOnlyList<DashboardKpiViewModel> BuildKpis(
        DashboardQueryContext context,
        DashboardTotals totals,
        DashboardComparedCounters counters,
        decimal? conversionRate,
        decimal? previousConversionRate)
    {
        var current = counters.Current;
        var previous = counters.Previous;

        var kpis = new List<DashboardKpiViewModel>
        {
            DashboardKpiFactory.Snapshot(
                "activeJobs",
                "Vagas ativas",
                totals.ActiveJobs,
                "Vagas abertas e não excluídas neste momento. Não depende do período: a vaga guarda "
                + "apenas o estado atual, sem histórico de abertura e fechamento."),

            DashboardKpiFactory.Comparable(
                "newApplications",
                "Candidaturas recebidas",
                current.NewApplications,
                previous.NewApplications,
                "Candidaturas cuja data de envio cai no período."),

            DashboardKpiFactory.Comparable(
                "newCandidates",
                "Novos candidatos",
                current.NewCandidates,
                previous.NewCandidates,
                context.Filter.Scope.IsPlatformWide
                    ? "Pessoas cuja primeira candidatura na plataforma ocorreu no período."
                    : "Pessoas cuja primeira candidatura a uma vaga desta empresa ocorreu no período."),

            DashboardKpiFactory.Comparable(
                "newJobs",
                "Vagas publicadas",
                current.NewJobs,
                previous.NewJobs,
                "Vagas cuja data de publicação cai no período."),

            DashboardKpiFactory.Comparable(
                "conversionRate",
                "Taxa de aprovação",
                conversionRate,
                previousConversionRate,
                "Candidaturas do período que chegaram à aprovação (aprovadas ou concluídas), sobre as "
                + "que participaram do processo, e foram canceladas, pela empresa ou pelo candidato, ficam "
                + "fora da base. Não é taxa de contratação: o domínio não registra contratação.",
                unit: "percent")
        };

        if (context.Filter.Scope.IsPlatformWide)
        {
            kpis.Add(DashboardKpiFactory.Snapshot(
                "activeCompanies",
                "Empresas com vagas ativas",
                totals.CompaniesWithActiveJobs,
                "Empresas com pelo menos uma vaga aberta. A empresa não tem estado próprio de "
                + "ativação, então esta é a única leitura de \"ativa\" que o domínio sustenta."));
        }

        return kpis;
    }

    /// <summary>
    /// Funil com as etapas que o domínio sustenta.
    /// </summary>
    /// <remarks>
    /// <para><b>"Aprovadas" soma Aprovada + Concluída.</b> A candidatura guarda apenas o status
    /// atual: quem foi aprovado e depois concluído hoje aparece como concluído. Contar só o status
    /// <c>Approved</c> faria a etapa encolher justamente quando o processo avança, o funil mostraria
    /// piora onde houve progresso.</para>
    ///
    /// <para><b>A base exclui as canceladas</b> — <c>Canceled</c> (a empresa encerrou a vaga ou
    /// descartou) e <c>CanceledByCandidate</c> (o candidato desistiu). Uma candidatura cancelada saiu
    /// do processo e nunca teve chance de chegar à aprovação; mantê-la na base infla o denominador e
    /// faz a taxa de conversão cair sem que nada tenha piorado no recrutamento. Com o cancelamento
    /// pelo candidato disponível na plataforma, esse volume passou a existir de verdade.</para>
    ///
    /// <para><b>Consequência a comunicar:</b> os números do funil e a taxa de conversão <b>mudam</b>
    /// face ao que o dashboard exibia antes - a base fica menor e a conversão sobe. Não é correcção de
    /// bug de cálculo, é mudança de definição da métrica, e quem acompanha a série histórica precisa
    /// de saber a data do corte.</para>
    /// </remarks>
    private static DashboardFunnelViewModel BuildFunnel(IReadOnlyList<DashboardStatusComparison> byStatus)
    {
        var applications = EffectiveApplications(byStatus);
        var reachedApproval =
            CountOf(byStatus, ApplicationStatusEnum.Approved) + CountOf(byStatus, ApplicationStatusEnum.Finished);
        var finished = CountOf(byStatus, ApplicationStatusEnum.Finished);

        var stages = new List<DashboardFunnelStageViewModel>
        {
            new("applications", "Candidaturas", applications, null, null),
            new("approved", "Chegaram à aprovação", reachedApproval, Share(reachedApproval, applications), Share(reachedApproval, applications)),
            new("finished", "Processos concluídos", finished, Share(finished, reachedApproval), Share(finished, applications))
        };

        return new DashboardFunnelViewModel(
            stages,
            ConversionRate(applications, reachedApproval),
            "O funil começa na candidatura: a plataforma não registra visualizações de vaga. "
            + "Candidaturas canceladas (pela empresa ou pelo candidato) não entram na base. "
            + "A última etapa é a conclusão do processo, que não distingue contratação de encerramento sem contratação.");
    }

    /// <summary>
    /// Base do funil: candidaturas que participaram do processo, sem as que saíram por cancelamento.
    /// </summary>
    private static int EffectiveApplications(
        IReadOnlyList<DashboardStatusComparison> byStatus,
        bool previousPeriod = false)
    {
        return byStatus
            .Where(item => item.Status is not (ApplicationStatusEnum.Canceled or ApplicationStatusEnum.CanceledByCandidate))
            .Sum(item => previousPeriod ? item.Previous : item.Current);
    }

    private static int CountOf(
        IReadOnlyList<DashboardStatusComparison> byStatus,
        ApplicationStatusEnum status,
        bool previousPeriod = false)
    {
        var row = byStatus.FirstOrDefault(item => item.Status == status);
        if (row is null)
        {
            return 0;
        }

        return previousPeriod ? row.Previous : row.Current;
    }

    private static decimal? ConversionRate(int applications, int reachedApproval)
        => Share(reachedApproval, applications);

    private static decimal? Share(int value, int baseline)
        => baseline <= 0 ? null : Math.Round(value * 100m / baseline, 1, MidpointRounding.AwayFromZero);
}
