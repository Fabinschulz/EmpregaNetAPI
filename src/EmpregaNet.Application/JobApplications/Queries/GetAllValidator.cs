using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.JobApplications.ViewModel;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed class GetAllJobApplicationsValidator : BasePaginatedQueryValidator<GetAllQuery<JobApplicationViewModel>>
{
    public GetAllJobApplicationsValidator() : base()
    {
    }
}
