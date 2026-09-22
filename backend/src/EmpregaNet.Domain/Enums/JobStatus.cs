using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Situação da vaga tal como é lida por quem consome a API.
    /// </summary>
    public enum JobStatusEnum
    {
        [Description("Ativa")] Active,
        [Description("Encerrada pela empresa")] ClosedManually,
        [Description("Encerrada por preenchimento")] ClosedByFulfillment,
    }

    public static class JobStatus
    {
        /// <param name="isActive">A vaga ainda está aberta?</param>
        /// <param name="closureReason">
        /// Razão do encerramento da vaga. Se a vaga ainda estiver aberta, este parâmetro será nulo.
        /// </param>
        public static JobStatusEnum Resolve(bool isActive, JobClosureReasonEnum? closureReason)
        {
            if (isActive)
            {
                return JobStatusEnum.Active;
            }

            return closureReason == JobClosureReasonEnum.Fulfilled
                ? JobStatusEnum.ClosedByFulfillment
                : JobStatusEnum.ClosedManually;
        }
    }
}
