using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Por que uma vaga deixou de estar activa. <c>null</c> na coluna significa "ainda aberta".
    /// </summary>
    public enum JobClosureReasonEnum
    {
        /// <summary>Acto do recrutamento: alguém encerrou a vaga pelo botão.</summary>
        [Description("Encerrada pela empresa")] Manual,

        /// <summary>
        /// Consequência automática: as posições disponíveis foram todas preenchidas.
        /// </summary>
        [Description("Encerrada por preenchimento das posições")] Fulfilled,
    }
}
