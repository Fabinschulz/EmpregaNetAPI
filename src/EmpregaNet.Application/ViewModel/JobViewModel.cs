using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.ViewModel
{
    public sealed class JobViewModel
    {
        public long Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Salary { get; set; }
        public JobTypeEnum JobType { get; set; }
        public DateTime PublicationDate { get; set; }
        public long CompanyId { get; set; }
        public Company? Company { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;
        public DateTime? DeletedAt { get; set; } = null;
        public bool IsDeleted { get; set; } = false;

    }
}