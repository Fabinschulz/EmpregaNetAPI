using System.Globalization;
using EmpregaNet.Application.Dashboard.ViewModel;

namespace EmpregaNet.Application.Dashboard.UseCase;

/// <summary>
/// Números crus de que as leituras derivadas precisam.
/// </summary>
/// <param name="ActiveJobs">Vagas abertas no fim do período - base das leituras de saúde.</param>
/// <param name="StaleJobs">Vagas ativas sem candidatura na janela de estagnação.</param>
/// <param name="StaleDays">Janela, em dias, usada para considerar a vaga estagnada.</param>
/// <param name="JobsWithoutApplications">Vagas ativas que nunca receberam candidatura.</param>
/// <param name="Applications">Candidaturas recebidas no período.</param>
/// <param name="PreviousApplications">Candidaturas recebidas no período anterior.</param>
/// <param name="NewCandidates">Candidatos que entraram no funil no período.</param>
/// <param name="PreviousNewCandidates">Candidatos que entraram no funil no período anterior.</param>
/// <param name="TopApplicationArea">Área com mais candidaturas no período; nulo se não houve candidatura.</param>
/// <param name="TopApplicationAreaCount">Candidaturas da área líder.</param>
/// <param name="CategorizedApplications">Candidaturas do período distribuídas por área.</param>
/// <param name="TopJobTitle">Vaga com mais candidaturas acumuladas; nulo se não há vaga.</param>
/// <param name="TopJobApplications">Candidaturas acumuladas da vaga líder.</param>
/// <param name="AverageApplicationsPerJob">Média de candidaturas por vaga no mesmo universo.</param>
public sealed record DashboardInsightsInput(
    int ActiveJobs,
    int StaleJobs,
    int StaleDays,
    int JobsWithoutApplications,
    int Applications,
    int PreviousApplications,
    int NewCandidates,
    int PreviousNewCandidates,
    string? TopApplicationArea,
    int TopApplicationAreaCount,
    int CategorizedApplications,
    string? TopJobTitle,
    int TopJobApplications,
    double AverageApplicationsPerJob);

/// <summary>
/// Compõe as leituras derivadas exibidas no painel de insights.
/// </summary>
public static class DashboardInsightsBuilder
{
    public const decimal AreaConcentrationThreshold = 25m;
    public const decimal JobAboveAverageThreshold = 15m;
    public const decimal PeriodChangeThreshold = 10m;

    /// <summary>Proporção de vagas abertas sem candidatura a partir da qual o número vira alerta.</summary>
    public const decimal IdleJobsShareThreshold = 20m;

    /// <summary>Proporção de vagas estagnadas que eleva o alerta a severidade alta.</summary>
    private const decimal StaleJobsHighSeverityShare = 33m;

    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static IReadOnlyList<DashboardInsightViewModel> Build(DashboardInsightsInput input)
    {
        var insights = new List<DashboardInsightViewModel>();

        AddStaleJobs(insights, input);
        AddIdleJobs(insights, input);
        AddApplicationsChange(insights, input);
        AddCandidatesGrowth(insights, input);
        AddTopJobPerformance(insights, input);
        AddAreaConcentration(insights, input);

        return insights;
    }

    /// <summary>Vagas abertas que pararam de receber candidaturas - o alerta mais acionável.</summary>
    private static void AddStaleJobs(List<DashboardInsightViewModel> insights, DashboardInsightsInput input)
    {
        if (input.StaleJobs <= 0)
        {
            return;
        }

        var isSingular = input.StaleJobs == 1;
        var share = Share(input.StaleJobs, input.ActiveJobs);

        var message = isSingular
            ? $"1 vaga ativa está há mais de {input.StaleDays} dias sem novas candidaturas."
            : $"{Format(input.StaleJobs)} vagas ativas estão há mais de {input.StaleDays} dias sem novas candidaturas.";

        if (share is { } value)
        {
            message += $" São {Format(value)}% das vagas abertas.";
        }

        insights.Add(new DashboardInsightViewModel(
            "staleJobs",
            "attention",
            "warning",
            share >= StaleJobsHighSeverityShare ? "high" : "medium",
            isSingular ? "1 vaga sem movimento" : $"{Format(input.StaleJobs)} vagas sem movimento",
            message));
    }

    /// <summary>
    /// Vagas abertas que nunca receberam candidatura.
    /// </summary>
    private static void AddIdleJobs(List<DashboardInsightViewModel> insights, DashboardInsightsInput input)
    {
        if (input.JobsWithoutApplications <= 0 || input.ActiveJobs <= 0)
        {
            return;
        }

        var share = Share(input.JobsWithoutApplications, input.ActiveJobs);
        if (share is null || share < IdleJobsShareThreshold)
        {
            return;
        }

        var isSingular = input.JobsWithoutApplications == 1;
        var subject = isSingular ? "1 vaga aberta" : $"{Format(input.JobsWithoutApplications)} vagas abertas";

        insights.Add(new DashboardInsightViewModel(
            "jobsWithoutApplications",
            "attention",
            "warning",
            "low",
            isSingular
                ? "1 vaga sem nenhuma candidatura"
                : $"{Format(input.JobsWithoutApplications)} vagas sem nenhuma candidatura",
            $"{subject} ainda não recebeu nenhuma candidatura - {Format(share.Value)}% das vagas abertas."));
    }

    /// <summary>Movimento das candidaturas contra o período anterior, para cima ou para baixo.</summary>
    private static void AddApplicationsChange(List<DashboardInsightViewModel> insights, DashboardInsightsInput input)
    {
        var change = DashboardKpiFactory.ChangePercent(input.Applications, input.PreviousApplications);
        if (change is null || Math.Abs(change.Value) < PeriodChangeThreshold)
        {
            return;
        }

        var magnitude = Math.Abs(change.Value);
        var comparison = $"O período recebeu {Format(input.Applications)} candidaturas, contra "
            + $"{Format(input.PreviousApplications)} no período anterior.";

        if (change.Value > 0)
        {
            insights.Add(new DashboardInsightViewModel(
                "applicationsGrowth",
                "growth",
                "positive",
                "medium",
                $"Candidaturas em alta: {Format(magnitude)}%",
                comparison));

            return;
        }

        // Queda é problema, não "crescimento negativo": entra na coluna de atenção, onde o
        // utilizador procura o que precisa de ação.
        insights.Add(new DashboardInsightViewModel(
            "applicationsDrop",
            "attention",
            "negative",
            magnitude >= 2 * PeriodChangeThreshold ? "high" : "medium",
            $"Candidaturas em queda: {Format(magnitude)}%",
            comparison));
    }

    /// <summary>Entrada de novos candidatos no funil, quando cresce acima do limiar.</summary>
    private static void AddCandidatesGrowth(List<DashboardInsightViewModel> insights, DashboardInsightsInput input)
    {
        var change = DashboardKpiFactory.ChangePercent(input.NewCandidates, input.PreviousNewCandidates);
        if (change is null || change.Value < PeriodChangeThreshold)
        {
            return;
        }

        insights.Add(new DashboardInsightViewModel(
            "candidatesGrowth",
            "growth",
            "positive",
            "low",
            $"Novos candidatos: {Format(change.Value)}%",
            $"{Format(input.NewCandidates)} pessoas entraram no funil no período, contra "
            + $"{Format(input.PreviousNewCandidates)} no anterior."));
    }

    /// <summary>A vaga que puxa o volume, quando se destaca da média de forma relevante.</summary>
    private static void AddTopJobPerformance(List<DashboardInsightViewModel> insights, DashboardInsightsInput input)
    {
        if (input.TopJobTitle is null || input.AverageApplicationsPerJob <= 0 || input.TopJobApplications <= 0)
        {
            return;
        }

        var average = (decimal)input.AverageApplicationsPerJob;
        var deviation = Math.Round(
            (input.TopJobApplications - average) * 100m / average,
            0,
            MidpointRounding.AwayFromZero);

        if (deviation < JobAboveAverageThreshold)
        {
            return;
        }

        insights.Add(new DashboardInsightViewModel(
            "topJobAboveAverage",
            "highlight",
            "positive",
            "low",
            "Vaga acima da média",
            $"A vaga \"{input.TopJobTitle}\" tem {Format(deviation)}% mais candidaturas que a média das vagas."));
    }

    /// <summary>Concentração das candidaturas numa área - padrão da operação, não problema.</summary>
    private static void AddAreaConcentration(List<DashboardInsightViewModel> insights, DashboardInsightsInput input)
    {
        if (input.TopApplicationArea is null || input.CategorizedApplications <= 0)
        {
            return;
        }

        var share = Share(input.TopApplicationAreaCount, input.CategorizedApplications);
        if (share is null || share < AreaConcentrationThreshold)
        {
            return;
        }

        insights.Add(new DashboardInsightViewModel(
            "areaConcentration",
            "behavior",
            "neutral",
            "low",
            $"{input.TopApplicationArea} concentra a procura",
            $"A área {input.TopApplicationArea} concentra {Format(share.Value)}% das candidaturas do período."));
    }

    /// <summary>Percentagem de um valor sobre a base; nulo quando a base é zero.</summary>
    private static decimal? Share(int value, int baseline)
        => baseline <= 0 ? null : Math.Round(value * 100m / baseline, 1, MidpointRounding.AwayFromZero);

    private static string Format(int value) => value.ToString("N0", BrazilianCulture);

    private static string Format(decimal value) => value == Math.Truncate(value)
        ? value.ToString("N0", BrazilianCulture)
        : value.ToString("N1", BrazilianCulture);
}
