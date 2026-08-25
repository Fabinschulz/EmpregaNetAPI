using EmpregaNet.Application.Dashboard.ViewModel;

namespace EmpregaNet.Application.Dashboard.UseCase;

/// <summary>
/// Métricas que o produto pede e este domínio ainda não sustenta.
/// </summary>
public static class DashboardDomainGaps
{
    public static readonly DashboardUnavailableMetricViewModel JobViews = new(
        "jobViews",
        "Visualizações de vaga",
        "A plataforma não registra visualizações: não existe entidade nem contador de exibição da vaga. "
        + "O funil começa, por isso, na candidatura.");

    public static readonly DashboardUnavailableMetricViewModel Hires = new(
        "hires",
        "Contratações",
        "Não há status de contratação. O fluxo da candidatura termina em \"Concluída\", que não distingue "
        + "contratado de processo encerrado sem contratação - a taxa de conversão mede aprovação, não contratação.");

    public static readonly DashboardUnavailableMetricViewModel JobExpiration = new(
        "jobsNearExpiration",
        "Vagas próximas do vencimento",
        "A vaga não tem data de expiração: encerra por ação do recrutador. O indicador disponível é o "
        + "tempo desde a publicação e o tempo sem novas candidaturas.");

    public static readonly DashboardUnavailableMetricViewModel CandidateArea = new(
        "candidatesByArea",
        "Candidatos por área de atuação",
        "O cadastro do candidato não tem área nem profissão. A concentração por área é medida pelas vagas "
        + "a que ele se candidatou, não pelo perfil dele.");



    public static readonly DashboardUnavailableMetricViewModel ApplicationHistory = new(
        "applicationStageHistory",
        "Histórico de etapas da candidatura",
        "A candidatura guarda apenas o status atual, sem histórico de transições. O funil soma \"Aprovada\" "
        + "e \"Concluída\" para estimar quantas chegaram à aprovação.");


    /// <summary>Lacunas relevantes para a visão geral (indicadores e funil).</summary>
    public static readonly IReadOnlyList<DashboardUnavailableMetricViewModel> Overview =
        [JobViews, Hires, ApplicationHistory];

    /// <summary>Lacunas relevantes para as distribuições.</summary>
    public static readonly IReadOnlyList<DashboardUnavailableMetricViewModel> Distribution =
        [CandidateArea];

    /// <summary>Lacunas relevantes para o ranking de vagas.</summary>
    public static readonly IReadOnlyList<DashboardUnavailableMetricViewModel> Jobs =
        [JobViews, JobExpiration];

    /// <summary>Lacunas relevantes para as séries temporais.</summary>
    public static readonly IReadOnlyList<DashboardUnavailableMetricViewModel> Trends =
        [ApplicationHistory];
}
