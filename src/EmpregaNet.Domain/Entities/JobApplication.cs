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
        public long JobId { get; private set; }
        public long UserId { get; private set; }
        public ApplicationStatusEnum Status { get; private set; }
        public DateTimeOffset AppliedAt { get; private set; }

        private JobApplication() { }

        public JobApplication(long jobId, long userId)
        {
            JobId = jobId;
            UserId = userId;
            Status = ApplicationStatusEnum.Processing;
            AppliedAt = DateTimeOffset.UtcNow;
        }

        public void ChangeStatus(ApplicationStatusEnum status)
        {
            Status = status;
        }
    }
}