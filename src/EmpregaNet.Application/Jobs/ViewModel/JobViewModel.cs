using Mapper.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Companies.ViewModel;

namespace EmpregaNet.Application.Jobs.ViewModel;

public sealed class JobViewModel : IMapFrom<Job>, ICustomMap
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public JobTypeEnum JobType { get; set; }
    public DateTime PublicationDate { get; set; }
    public long CompanyId { get; set; }
    public CompanyViewModel? Company { get; set; }
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } = null;
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    public void CustomMap(IMapperConfigurationExpression configuration)
    {
        configuration.CreateMapWithOptions<Job, JobViewModel>().Apply();
    }

}
