using System.Diagnostics.CodeAnalysis;
using DomainCompany = EmpregaNet.Domain.Entities.Company;

namespace EmpregaNet.Application.Admin.Company.ViewModel;

/// <summary>
/// Projeção enxuta de empresa para preencher seletores (ex.: escolher a empresa ao publicar uma vaga).
/// </summary>
public sealed class CompanyOptionViewModel
{
    public long Id { get; set; }
    public required string Name { get; set; }
}

[ExcludeFromCodeCoverage]
public static class CompanyOptionMapper
{
    public static CompanyOptionViewModel ToOptionViewModel(this DomainCompany entity) =>
        new() { Id = entity.Id, Name = entity.CompanyName };
}
