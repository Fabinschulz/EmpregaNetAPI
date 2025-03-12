using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o status de uma candidatura a uma vaga.
    /// </summary>
    public enum ApplicationStatus
    {
        [Description("Aprovado")] Approved = 0,
        [Description("Aguardando aprovação")] Pending = 1,
        [Description("Rejeitado")] Rejected = 2,
        [Description("Expirado")] Timeout = 3,
        [Description("Vaga cancelada pela empresa")] Canceled = 4,
        [Description("Erro")] Error = 7,
        [Description("Em Análise")] Processing = 8,
        [Description("Encerrado")] Finished = 9,
    }
}