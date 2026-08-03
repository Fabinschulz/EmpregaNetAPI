using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Janela de publicação oferecida ao candidato no feed de vagas.
    /// </summary>
    /// <remarks>
    /// <see cref="Today"/> e <see cref="Last24Hours"/> são janelas distintas: "hoje" começa à
    /// meia-noite de Brasília, "últimas 24 horas" conta para trás a partir de agora. Por isso é enum
    /// e não um inteiro de dias.
    /// </remarks>
    public enum JobPublishedWindowEnum
    {
        /// <summary>Sem recorte por data (padrão).</summary>
        [Description("Qualquer data")] Any = 0,

        [Description("Hoje")] Today,
        [Description("Últimas 24 horas")] Last24Hours,
        [Description("Últimos 3 dias")] Last3Days,
        [Description("Últimos 7 dias")] Last7Days,
        [Description("Últimos 15 dias")] Last15Days,
        [Description("Últimos 30 dias")] Last30Days
    }
}
