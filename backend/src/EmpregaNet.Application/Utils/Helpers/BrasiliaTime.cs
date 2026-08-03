using System.Globalization;

namespace EmpregaNet.Application.Utils.Helpers
{
    /// <summary>
    /// Fuso horário de Brasília e apresentação de datas ao utilizador.
    /// </summary>
    /// <remarks>
    /// Datas são persistidas e comparadas em UTC; a conversão para o fuso local acontece apenas na
    /// borda de apresentação (ViewModels) e no cálculo de "hoje" nos filtros por data.
    /// </remarks>
    public static class BrasiliaTime
    {
        private const string IanaId = "America/Sao_Paulo";
        private const string WindowsId = "E. South America Standard Time";

        /// <summary>Formato padrão exibido ao utilizador.</summary>
        public const string DefaultFormat = "dd/MM/yyyy HH:mm:ss";

        private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

        /// <summary>
        /// Resolve o fuso de forma portável: o id IANA existe no Linux/macOS e no Windows moderno,
        /// o id do Windows cobre versões que não trazem a base IANA.
        /// </summary>
        private static readonly TimeZoneInfo TimeZone = Resolve();

        /// <inheritdoc cref="TimeZone"/>
        public static TimeZoneInfo GetBrasiliaTimeZone() => TimeZone;

        /// <summary>
        /// Converte uma data UTC para o fuso de Brasília e formata em pt-BR.
        /// </summary>
        /// <param name="utcDate">Data lida do banco, em UTC.</param>
        /// <param name="format">Formato desejado; por omissão <see cref="DefaultFormat"/>.</param>
        /// <returns>A data formatada, ou string vazia quando não há data.</returns>
        public static string Format(DateTimeOffset? utcDate, string format = DefaultFormat)
        {
            if (!utcDate.HasValue)
            {
                return string.Empty;
            }

            return TimeZoneInfo.ConvertTime(utcDate.Value, TimeZone).ToString(format, BrazilianCulture);
        }

        private static TimeZoneInfo Resolve()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(IanaId);
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(WindowsId);
            }
        }
    }
}
