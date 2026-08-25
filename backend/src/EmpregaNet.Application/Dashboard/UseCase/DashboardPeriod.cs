using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Common.Dashboard;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Dashboard.UseCase;

/// <summary>
/// Período de análise resolvido: o intervalo UTC que vai ao banco e as datas locais que a tela exibe.
/// </summary>
/// <param name="Range">Intervalo semiaberto em UTC.</param>
/// <param name="LocalOffset">Deslocamento do fuso usado no agrupamento por dia.</param>
/// <param name="FromLocal">Primeiro dia do período, no fuso de Brasília.</param>
/// <param name="ToLocalInclusive">Último dia do período (inclusivo), no fuso de Brasília.</param>
public sealed record DashboardResolvedPeriod(
    DashboardDateRange Range,
    TimeSpan LocalOffset,
    DateOnly FromLocal,
    DateOnly ToLocalInclusive)
{
    /// <summary>Dias inteiros cobertos pelo período; base da granularidade padrão.</summary>
    public int DayCount => ToLocalInclusive.DayNumber - FromLocal.DayNumber + 1;
}

/// <summary>
/// Traduz o período pedido no cabeçalho do dashboard em fronteiras UTC.
/// </summary>
public interface IDashboardPeriodResolver
{
    DashboardResolvedPeriod Resolve(DashboardPeriodEnum period, DateOnly? from, DateOnly? to);
    DashboardGranularityEnum ResolveGranularity(DashboardResolvedPeriod period, DashboardGranularityEnum? requested);
}

/// <inheritdoc cref="IDashboardPeriodResolver"/>
public sealed class DashboardPeriodResolver : IDashboardPeriodResolver
{
    public const int MaxCustomRangeDays = 366;

    /// <summary>Acima deste número de dias a série diária vira ruído visual e passa a semanal.</summary>
    private const int DailyGranularityMaxDays = 31;

    /// <summary>Acima deste número de dias a série semanal também fica densa e passa a mensal.</summary>
    private const int WeeklyGranularityMaxDays = 120;

    private readonly TimeProvider _timeProvider;

    public DashboardPeriodResolver(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public DashboardResolvedPeriod Resolve(DashboardPeriodEnum period, DateOnly? from, DateOnly? to)
    {
        var timeZone = BrasiliaTime.GetBrasiliaTimeZone();
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), timeZone).DateTime);

        var (fromLocal, toLocal) = period switch
        {
            DashboardPeriodEnum.Today => (today, today),
            DashboardPeriodEnum.Last7Days => (today.AddDays(-6), today),
            DashboardPeriodEnum.Last30Days => (today.AddDays(-29), today),
            DashboardPeriodEnum.Last90Days => (today.AddDays(-89), today),
            DashboardPeriodEnum.ThisYear => (new DateOnly(today.Year, 1, 1), today),
            DashboardPeriodEnum.Custom => ResolveCustom(from, to),
            _ => (today.AddDays(-29), today)
        };

        var fromUtc = StartOfLocalDayUtc(fromLocal, timeZone);
        var toUtcExclusive = StartOfLocalDayUtc(toLocal.AddDays(1), timeZone);

        return new DashboardResolvedPeriod(
            new DashboardDateRange(fromUtc, toUtcExclusive),
            timeZone.GetUtcOffset(fromUtc),
            fromLocal,
            toLocal);
    }

    public DashboardGranularityEnum ResolveGranularity(
        DashboardResolvedPeriod period,
        DashboardGranularityEnum? requested)
    {
        if (requested is { } granularity)
        {
            return granularity;
        }

        return period.DayCount switch
        {
            <= DailyGranularityMaxDays => DashboardGranularityEnum.Daily,
            <= WeeklyGranularityMaxDays => DashboardGranularityEnum.Weekly,
            _ => DashboardGranularityEnum.Monthly
        };
    }

    private static (DateOnly From, DateOnly To) ResolveCustom(DateOnly? from, DateOnly? to)
    {
        if (from is null || to is null)
        {
            throw new ValidationAppException(
                "period",
                "Informe as datas inicial e final para o período personalizado.",
                DomainErrorEnum.INVALID_QUERY_FILTER);
        }

        if (from > to)
        {
            throw new ValidationAppException(
                "from",
                "A data inicial não pode ser posterior à data final.",
                DomainErrorEnum.INVALID_QUERY_FILTER);
        }

        var days = to.Value.DayNumber - from.Value.DayNumber + 1;
        if (days > MaxCustomRangeDays)
        {
            throw new ValidationAppException(
                "to",
                $"O período personalizado não pode exceder {MaxCustomRangeDays} dias.",
                DomainErrorEnum.INVALID_QUERY_FILTER);
        }

        return (from.Value, to.Value);
    }

    /// <summary>
    /// Meia-noite local do dia informado, convertida para UTC.
    /// </summary>
    /// <remarks>
    /// <c>GetUtcOffset</c> é consultado para a data em questão, não para agora: se o horário de
    /// verão voltar, a fronteira de um dia de janeiro continua correta ao ser pedida em julho.
    /// </remarks>
    private static DateTimeOffset StartOfLocalDayUtc(DateOnly day, TimeZoneInfo timeZone)
    {
        var midnight = day.ToDateTime(TimeOnly.MinValue);
        var offset = timeZone.GetUtcOffset(DateTime.SpecifyKind(midnight, DateTimeKind.Unspecified));

        return new DateTimeOffset(midnight, offset).ToUniversalTime();
    }
}
