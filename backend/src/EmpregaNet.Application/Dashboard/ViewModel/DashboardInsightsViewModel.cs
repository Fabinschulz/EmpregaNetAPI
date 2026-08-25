namespace EmpregaNet.Application.Dashboard.ViewModel;

/// <summary>
/// Leituras derivadas do período - o painel de insights da tela.
/// </summary>
/// <remarks>
/// Endpoint próprio, e não um campo da visão geral, por duas razões concretas: as consultas que o
/// alimentam são diferentes das dos indicadores (estagnação, concentração, desvio da média) e não
/// devem atrasar o topo da tela; e uma delas falhar não deve derrubar os cartões, que são a
/// informação principal.
/// </remarks>
public sealed class DashboardInsightsViewModel
{
    public required DashboardMetaViewModel Meta { get; init; }
    public required IReadOnlyList<DashboardInsightViewModel> Items { get; init; }
    public required int StaleDays { get; init; }
}
