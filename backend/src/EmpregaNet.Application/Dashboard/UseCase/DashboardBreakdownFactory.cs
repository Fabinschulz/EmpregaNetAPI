using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common.Dashboard;

namespace EmpregaNet.Application.Dashboard.UseCase;

/// <summary>
/// Converte contagens por enum em distribuições prontas para gráfico.
/// </summary>
public static class DashboardBreakdownFactory
{
    /// <summary>Chave da fatia que agrega a cauda longa.</summary>
    public const string OthersKey = "Others";

    /// <summary>
    /// Monta a distribuição a partir das contagens por membro de enum.
    /// </summary>
    public static DashboardBreakdownViewModel Create<TEnum>(
        IReadOnlyList<DashboardEnumCount<TEnum>> counts,
        int? total = null,
        TEnum? neutral = null,
        int? topN = null,
        string? uncategorizedNote = null)
        where TEnum : struct, Enum
    {
        var relevant = counts
            .Where(count => neutral is null || !count.Value.Equals(neutral.Value))
            .Where(count => count.Count > 0)
            .ToList();

        var categorized = relevant.Sum(count => count.Count);
        var universe = total ?? categorized;

        var ordered = relevant
            .Select(count => (
                Key: count.Value.ToString(),
                Label: ((Enum)(object)count.Value).ToDescription(),
                count.Count))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var items = topN is { } limit && ordered.Count > limit
            ? WithOthers(ordered, limit)
            : ordered;

        var note = universe > categorized && uncategorizedNote is not null
            ? string.Format(uncategorizedNote, universe - categorized)
            : null;

        return new DashboardBreakdownViewModel(
            [.. items.Select(item => new DashboardBreakdownItemViewModel(
                item.Key,
                item.Label,
                item.Count,
                Share(item.Count, categorized)))],
            universe,
            categorized,
            note);
    }

    /// <summary>
    /// Distribuição já montada a partir de pares chave/rótulo - para dimensões que não são enum.
    /// </summary>
    public static DashboardBreakdownViewModel FromItems(
        IReadOnlyList<(string Key, string Label, int Count)> items,
        int? total = null,
        int? topN = null,
        string? uncategorizedNote = null)
    {
        var relevant = items.Where(item => item.Count > 0).ToList();
        var categorized = relevant.Sum(item => item.Count);
        var universe = total ?? categorized;

        var ordered = relevant
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var trimmed = topN is { } limit && ordered.Count > limit
            ? WithOthers(ordered, limit)
            : ordered;

        var note = universe > categorized && uncategorizedNote is not null
            ? string.Format(uncategorizedNote, universe - categorized)
            : null;

        return new DashboardBreakdownViewModel(
            [.. trimmed.Select(item => new DashboardBreakdownItemViewModel(
                item.Key,
                item.Label,
                item.Count,
                Share(item.Count, categorized)))],
            universe,
            categorized,
            note);
    }

    private static List<(string Key, string Label, int Count)> WithOthers(
        List<(string Key, string Label, int Count)> ordered,
        int limit)
    {
        var head = ordered.Take(limit).ToList();
        var tail = ordered.Skip(limit).ToList();

        head.Add((OthersKey, $"Outras ({tail.Count})", tail.Sum(item => item.Count)));
        return head;
    }

    /// <summary>
    /// Percentagem sobre o que foi categorizado, com uma casa decimal.
    /// </summary>
    private static decimal Share(int value, int categorized)
        => categorized == 0 ? 0m : Math.Round(value * 100m / categorized, 1, MidpointRounding.AwayFromZero);
}
