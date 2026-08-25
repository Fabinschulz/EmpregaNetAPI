using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    public enum DashboardGranularityEnum
    {
        [Description("Diário")] Daily = 0,
        [Description("Semanal")] Weekly = 1,
        [Description("Mensal")] Monthly = 2
    }
}
