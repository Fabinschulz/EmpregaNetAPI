using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Admin.Company.ViewModel;

namespace EmpregaNet.Application.Admin.Company.Queries;

public sealed class CompanyViewModelGetAllValidator : BasePaginatedQueryValidator<GetAllQuery<CompanyViewModel>>
{
    public CompanyViewModelGetAllValidator() : base()
    {
    }
}
