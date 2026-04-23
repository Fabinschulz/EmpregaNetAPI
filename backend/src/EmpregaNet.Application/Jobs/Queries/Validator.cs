using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Jobs.ViewModel;

public sealed class JobViewModelGetAllValidator : BasePaginatedQueryValidator<GetAllQuery<JobViewModel>>
{
    public JobViewModelGetAllValidator() : base()
    {
    }
}