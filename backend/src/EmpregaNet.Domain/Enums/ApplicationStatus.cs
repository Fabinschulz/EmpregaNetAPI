using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o status de uma candidatura a uma vaga.
    /// </summary>
    /// <remarks>
    /// <b>A ordem é contrato de dados.</b> Nenhum membro declara valor explícito e a coluna
    /// <c>Status</c> é persistida como <c>integer</c>, portanto o significado de cada linha já
    /// gravada é a posição do membro nesta lista. Inserir um valor no meio reescreveria em silêncio
    /// o status de candidaturas existentes. <b>Valor novo entra sempre no fim.</b>
    /// </remarks>
    public enum ApplicationStatusEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Aprovado")] Approved,
        [Description("Recebida")] Pending,
        [Description("Rejeitado")] Rejected,
        [Description("Expirado")] Timeout,

        /// <summary>Ato da empresa: a vaga foi encerrada ou a candidatura descartada pelo recrutamento.</summary>
        [Description("Vaga cancelada pela empresa")] Canceled,
        [Description("Erro")] Error,
        [Description("Em Análise")] Processing,
        [Description("Encerrado")] Finished,

        /// <summary>
        /// Ato do candidato: ele próprio desistiu do processo. Distinto de <see cref="Canceled"/>,
        /// que é o ato da empresa, e a autoria fica no status, sem coluna nem <i>join</i> extra.
        /// </summary>
        [Description("Cancelada pelo candidato")] CanceledByCandidate,
    }
}
