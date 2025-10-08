using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Companies.ViewModel;

public sealed class CompanyViewModelGetAllValidator : BasePaginatedQueryValidator<GetAllQuery<CompanyViewModel>>
{
    public CompanyViewModelGetAllValidator() : base()
    {
    }
}