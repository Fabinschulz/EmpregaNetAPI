using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Janela de análise oferecida no cabeçalho do dashboard.
    /// </summary>
    public enum DashboardPeriodEnum
    {
        [Description("Hoje")] Today = 0,
        [Description("Últimos 7 dias")] Last7Days = 1,
        [Description("Últimos 30 dias")] Last30Days = 2,
        [Description("Últimos 90 dias")] Last90Days = 3,
        [Description("Este ano")] ThisYear = 4,

        /// <summary>Intervalo informado pelo utilizador em <c>from</c>/<c>to</c>.</summary>
        [Description("Personalizado")] Custom = 5
    }
}
