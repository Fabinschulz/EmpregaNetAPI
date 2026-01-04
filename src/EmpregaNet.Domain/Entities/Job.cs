using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Domain.Entities
{
    public class Job : BaseEntity, IAggregateRoot
    {
        public long CompanyId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public decimal Salary { get; private set; }
        public JobTypeEnum JobType { get; private set; }
        public DateTimeOffset PublishedAt { get; private set; }
        public bool IsActive { get; private set; }

        public Job(long companyId, string title, string description, decimal salary, JobTypeEnum jobType)
        {
            CompanyId = companyId;
            Title = title;
            Description = description;
            Salary = salary;
            JobType = jobType;
            PublishedAt = DateTimeOffset.UtcNow;
            IsActive = true;
        }

        public void UpdateJob(string title, string description, decimal salary, JobTypeEnum jobType)
        {
            Title = title;
            Description = description;
            Salary = salary;
            JobType = jobType;
        }

        public void Close() => IsActive = false;
    }
}