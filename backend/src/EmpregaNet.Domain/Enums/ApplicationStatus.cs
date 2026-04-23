using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o status de uma candidatura a uma vaga.
    /// </summary>
    public enum ApplicationStatusEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Aprovado")] Approved,
        [Description("Aguardando aprovação")] Pending,
        [Description("Rejeitado")] Rejected,
        [Description("Expirado")] Timeout,
        [Description("Vaga cancelada pela empresa")] Canceled,
        [Description("Erro")] Error,
        [Description("Em Análise")] Processing,
        [Description("Encerrado")] Finished,
    }
}