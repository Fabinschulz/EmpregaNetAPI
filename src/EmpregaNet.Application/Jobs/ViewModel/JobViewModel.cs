using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
using EmpregaNet.Application.Companies.ViewModel;

namespace EmpregaNet.Application.Jobs.ViewModel;

public sealed class JobViewModel
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public JobTypeEnum JobType { get; set; }
    public DateTime PublicationDate { get; set; }
    public long CompanyId { get; set; }
    public CompanyViewModel? Company { get; set; }
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>(); // toDo: substituir para JobApplicationViewModel
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } = null;
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

}

[ExcludeFromCodeCoverage]
public static class JobMapper
{
    public static JobViewModel ToViewModel(this Job entity)
    {
        return new JobViewModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Salary = entity.Salary,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt,
            IsDeleted = entity.IsDeleted
        };
    }
}
