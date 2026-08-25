using System.Globalization;
using EmpregaNet.Application.Dashboard.ViewModel;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Dashboard.UseCase;

public static class DashboardSeriesBuilder
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static DashboardSeriesViewModel Build(
        string key,
        string label,
        IReadOnlyList<DashboardDailyPoint> dailyPoints,
        DashboardResolvedPeriod period,
        DashboardGranularityEnum granularity)
    {
        var byDay = dailyPoints
            .GroupBy(point => point.Day)
            .ToDictionary(group => group.Key, group => group.Sum(point => point.Count));

        var points = new List<DashboardSeriesPointViewModel>();
        var total = 0;

        foreach (var (start, endInclusive) in Buckets(period, granularity))
        {
            var value = 0;
            for (var day = start; day <= endInclusive; day = day.AddDays(1))
            {
                value += byDay.GetValueOrDefault(day);
            }

            total += value;
            points.Add(new DashboardSeriesPointViewModel(
                start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                BucketLabel(start, granularity),
                value));
        }

        return new DashboardSeriesViewModel(key, label, total, points);
    }

    public static string GranularityLabel(DashboardGranularityEnum granularity) => granularity switch
    {
        DashboardGranularityEnum.Weekly => "Semanal (segunda a domingo)",
        DashboardGranularityEnum.Monthly => "Mensal",
        _ => "Diário"
    };

    private static IEnumerable<(DateOnly Start, DateOnly EndInclusive)> Buckets(
        DashboardResolvedPeriod period,
        DashboardGranularityEnum granularity)
    {
        var from = period.FromLocal;
        var to = period.ToLocalInclusive;

        var cursor = from;
        while (cursor <= to)
        {
            var naturalEnd = granularity switch
            {
                DashboardGranularityEnum.Weekly => EndOfWeek(cursor),
                DashboardGranularityEnum.Monthly => EndOfMonth(cursor),
                _ => cursor
            };

            var end = naturalEnd > to ? to : naturalEnd;
            yield return (cursor, end);
            cursor = end.AddDays(1);
        }
    }

    private static DateOnly EndOfWeek(DateOnly day)
    {
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)day.DayOfWeek + 7) % 7;
        return day.AddDays(daysUntilSunday);
    }

    private static DateOnly EndOfMonth(DateOnly day)
        => new(day.Year, day.Month, DateTime.DaysInMonth(day.Year, day.Month));

    private static string BucketLabel(DateOnly start, DashboardGranularityEnum granularity) => granularity switch
    {
        DashboardGranularityEnum.Monthly => start.ToString("MMM/yyyy", BrazilianCulture),
        _ => start.ToString("dd/MM", BrazilianCulture)
    };
}
