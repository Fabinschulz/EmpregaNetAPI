using EmpregaNet.Application.ViewModel;
using EmpregaNet.Domain.Components.Mapper.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Mapper.Interface;

namespace EmpregaNet.Application.Companies.ViewModel;

public sealed class CompanyViewModel : IMapFrom<Company>, ICustomMap
{
    public long Id { get; set; }
    public TypeOfActivityEnum? TypeOfActivity { get; set; }
    public required string CompanyName { get; set; }
    public Address? Address { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public ICollection<JobViewModel> Jobs { get; set; } = new List<JobViewModel>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } = null;
    public DateTime? DeletedAt { get; set; } = null;
    public bool IsDeleted { get; set; } = false;

    public void CustomMap(IMapperConfigurationExpression configuration)
    {
        configuration.CreateMapWithOptions<Company, CompanyViewModel>().Apply();
    }
}