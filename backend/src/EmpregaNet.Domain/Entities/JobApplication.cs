using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma candidatura de um usuário a uma vaga.
    /// </summary>
    public class JobApplication : BaseEntity, IAggregateRoot
    {
        /// <summary>
        /// Estados em que o candidato ainda pode desistir: o processo está em aberto e nenhum
        /// desfecho foi comunicado.
        /// </summary>
        private static readonly ApplicationStatusEnum[] CancelableByCandidate =
        [
            ApplicationStatusEnum.Pending,
            ApplicationStatusEnum.Processing
        ];

        public long JobId { get; private set; }
        public long UserId { get; private set; }
        public ApplicationStatusEnum Status { get; private set; }
        public DateTimeOffset AppliedAt { get; private set; }

        private JobApplication() { }

        /// <summary>
        /// Nasce em <see cref="ApplicationStatusEnum.Pending"/> ("Recebida"): registada, ainda não
        /// analisada.
        /// </summary>
        public JobApplication(long jobId, long userId)
        {
            JobId = jobId;
            UserId = userId;
            Status = ApplicationStatusEnum.Pending;
            AppliedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Transição conduzida pelo recrutamento.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Status inválido, status igual ao actual, destino reservado ao candidato, ou candidatura
        /// já cancelada pelo candidato.
        /// </exception>
        public void ChangeStatus(ApplicationStatusEnum status)
        {
            if (status == ApplicationStatusEnum.NaoSelecionado)
            {
                throw new InvalidOperationException("Status de candidatura inválido.");
            }

            if (Status == status)
            {
                throw new InvalidOperationException("A candidatura já está no status informado.");
            }

            if (status == ApplicationStatusEnum.CanceledByCandidate)
            {
                throw new InvalidOperationException(
                    "O cancelamento pelo candidato não pode ser aplicado pelo recrutamento.");
            }

            if (Status == ApplicationStatusEnum.CanceledByCandidate)
            {
                throw new InvalidOperationException(
                    "Esta candidatura foi cancelada pelo candidato e não pode mudar de status.");
            }

            Status = status;
        }

        /// <summary>
        /// Desistência declarada pelo próprio candidato. Permitida apenas enquanto o processo está
        /// em aberto (Recebida ou Em Análise).
        /// </summary>
        /// <exception cref="InvalidOperationException">Candidatura já num estado com desfecho.</exception>
        public void CancelByCandidate()
        {
            if (!CancelableByCandidate.Contains(Status))
            {
                throw new InvalidOperationException("Esta candidatura não pode mais ser cancelada.");
            }

            Status = ApplicationStatusEnum.CanceledByCandidate;
        }
    }
}
