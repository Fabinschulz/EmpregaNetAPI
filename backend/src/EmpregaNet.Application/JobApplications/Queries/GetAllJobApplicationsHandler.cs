using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.JobApplications.Queries;

public sealed class GetAllJobApplicationsHandler : IRequestHandler<GetAllQuery<JobApplicationViewModel>, ListDataPagination<JobApplicationViewModel>>
{
    private readonly IJobApplicationRepository _repository;
    private readonly IValidator<GetAllQuery<JobApplicationViewModel>> _validator;
    private readonly ILogger<GetAllJobApplicationsHandler> _logger;

    public GetAllJobApplicationsHandler(
        IJobApplicationRepository repository,
        IValidator<GetAllQuery<JobApplicationViewModel>> validator,
        ILogger<GetAllJobApplicationsHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ListDataPagination<JobApplicationViewModel>> Handle(GetAllQuery<JobApplicationViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando candidaturas (page: {Page}, size: {Size})", request.Page, request.Size);

        try
        {
            var result = await _repository.GetAllAsync(cancellationToken, request.Page, request.Size, request.OrderBy);
            var data = result.Data.Select(a => a.ToViewModel()).ToList();
            return new ListDataPagination<JobApplicationViewModel>(data, result.TotalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar todas as vagas aplicadas. Query: {@Query}", request);
            throw;
        }
    }
}
