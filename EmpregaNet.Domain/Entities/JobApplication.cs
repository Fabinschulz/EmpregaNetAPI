
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interface;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma candidatura de um usuário a uma vaga.
    /// </summary>
    public class JobApplication : BaseEntity, IAggregateRoot
    {
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public ApplicationStatus Status { get; set; }
        public long JobId { get; set; }
        public Job? Job { get; set; }
        public long UserId { get; set; }
        public User? User { get; set; }

    }
}