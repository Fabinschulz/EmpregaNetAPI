using EmpregaNet.Application.Dashboard.ViewModel;

namespace EmpregaNet.Application.Dashboard.UseCase;

/// <summary>
/// Monta os cartões de indicador, incluindo a comparação com o período anterior.
/// </summary>
public static class DashboardKpiFactory
{
    /// <summary>Indicador do período, com comparação contra o período anterior.</summary>
    /// <param name="key">Chave estável do indicador.</param>
    /// <param name="label">Rótulo em pt-BR.</param>
    /// <param name="value">Valor no período; nulo quando o indicador não tem base para existir.</param>
    /// <param name="previous">Valor no período anterior.</param>
    /// <param name="hint">Explicação da origem do número.</param>
    /// <param name="unit"><c>percent</c> quando o valor é taxa.</param>
    public static DashboardKpiViewModel Comparable(
        string key,
        string label,
        decimal? value,
        decimal? previous,
        string hint,
        string? unit = null)
    {
        var change = value is null ? null : ChangePercent(value.Value, previous);

        return new DashboardKpiViewModel
        {
            Key = key,
            Label = label,
            Value = value,
            Unit = unit,
            PreviousValue = value is null ? null : previous,
            ChangePercent = change,
            Trend = value is null ? "none" : Trend(value.Value, previous, change),
            Hint = hint,
            IsPeriodScoped = true
        };
    }

    /// <summary>
    /// Indicador acumulado, sem comparação.
    /// </summary>
    public static DashboardKpiViewModel Snapshot(
        string key,
        string label,
        decimal value,
        string hint,
        string? unit = null)
        => new()
        {
            Key = key,
            Label = label,
            Value = value,
            Unit = unit,
            PreviousValue = null,
            ChangePercent = null,
            Trend = "none",
            Hint = hint,
            IsPeriodScoped = false
        };

    /// <summary>
    /// Variação percentual, ou nulo quando não há base.
    /// </summary>
    public static decimal? ChangePercent(decimal value, decimal? previous)
    {
        if (previous is null || previous.Value == 0m)
        {
            return null;
        }

        return Math.Round((value - previous.Value) * 100m / previous.Value, 1, MidpointRounding.AwayFromZero);
    }

    private static string Trend(decimal value, decimal? previous, decimal? change)
    {
        if (previous is null)
        {
            return "none";
        }

        // Sem percentagem mas com base zero e valor positivo, a direção ainda é conhecida: subiu.
        if (change is null)
        {
            return value > 0m ? "up" : "flat";
        }

        return change.Value switch
        {
            > 0m => "up",
            < 0m => "down",
            _ => "flat"
        };
    }
}
