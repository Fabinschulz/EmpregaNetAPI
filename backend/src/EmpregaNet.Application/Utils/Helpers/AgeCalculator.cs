namespace EmpregaNet.Application.Utils.Helpers;

public static class AgeCalculator
{
    private const int MaxPlausibleAge = 120;

    /// <summary>
    /// Idade em anos completos no fuso de Brasília, ou <c>null</c> quando a data é ausente ou
    /// implausível.
    /// </summary>
    /// <remarks>
    /// Só <paramref name="now"/> é convertido de fuso. A data de nascimento é data de calendário,
    /// não instante: convertê-la moveria um nascimento gravado à meia-noite UTC para o dia anterior.
    /// </remarks>
    public static int? InCompleteYears(DateTimeOffset? birthDate, DateTimeOffset now)
    {
        if (birthDate is null)
        {
            return null;
        }

        var today = TimeZoneInfo.ConvertTime(now, BrasiliaTime.GetBrasiliaTimeZone()).Date;
        var birthday = birthDate.Value.Date;

        var years = today.Year - birthday.Year;
        if (today < birthday.AddYears(years))
        {
            years--;
        }

        return years is >= 0 and <= MaxPlausibleAge ? years : null;
    }
}
