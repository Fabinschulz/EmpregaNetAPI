using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interface;

namespace EmpregaNet.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma vaga de emprego publicada por uma empresa.
    /// </summary>
    public class Job : BaseEntity, IAggregateRoot
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Salary { get; set; }
        public JobType JobType { get; set; }
        public DateTime PublicationDate { get; set; }
        public long CompanyId { get; set; }
        public Company? Company { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

    }
}