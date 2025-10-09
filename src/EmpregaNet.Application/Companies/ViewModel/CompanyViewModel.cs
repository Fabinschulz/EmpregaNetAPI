using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Application.Jobs.ViewModel;
using System.Diagnostics.CodeAnalysis;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Utils.Helpers;

namespace EmpregaNet.Application.Companies.ViewModel;

public sealed class CompanyViewModel : BaseViewModel
{
    public long Id { get; set; }
    public TypeOfActivityEnum? TypeOfActivity { get; set; }
    public required string CompanyName { get; set; }
    public Address? Address { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public ICollection<JobViewModel> Jobs { get; set; } = new List<JobViewModel>();

}

[ExcludeFromCodeCoverage]
public static class CompanyMapper
{
    public static CompanyViewModel ToViewModel(this Company entity)
    {
        return new CompanyViewModel
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            TypeOfActivity = entity.TypeOfActivity,
            Address = entity.Address,
            RegistrationNumber = entity.RegistrationNumber.FormatCNPJ(),
            Email = entity.Email,
            Phone = entity.Phone,
            Jobs = entity.Jobs?.Select(job => job.ToViewModel()).ToList() ?? new List<JobViewModel>(),
            CreatedAtUtc = entity.CreatedAt,
            UpdatedAtUtc = entity.UpdatedAt,
            DeletedAtUtc = entity.DeletedAt,
            IsDeleted = entity.IsDeleted
        };
    }
}