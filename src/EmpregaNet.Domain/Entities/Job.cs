using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Entities
{
    public class Job : BaseEntity, IAggregateRoot
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public decimal Salary { get; private set; }
        public JobTypeEnum JobType { get; private set; }
        public DateTime PublicationDate { get; private set; }
        public long? CompanyId { get; private set; }
        public Company? Company { get; private set; }
        public ICollection<JobApplication> Applications { get; private set; } = new List<JobApplication>();

        public Job(string title, string description, decimal salary, JobTypeEnum jobType, long? companyId)
        {
            Title = title;
            Description = description;
            Salary = salary;
            JobType = jobType;
            CompanyId = companyId;
            PublicationDate = DateTime.UtcNow;
        }

        public void UpdateDetails(string title, string description, decimal salary, JobTypeEnum jobType)
        {
            Title = title;
            Description = description;
            Salary = salary;
            JobType = jobType;
        }
    }
}