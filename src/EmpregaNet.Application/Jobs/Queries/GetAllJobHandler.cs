using EmpregaNet.Application.Auth;
using EmpregaNet.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Admin.Company.ViewModel;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Jobs.ViewModel;

namespace EmpregaNet.Application.Jobs.Queries;

public sealed class GetAllValidator : BasePaginatedQueryValidator<GetAllQuery<JobViewModel>>
{
    public GetAllValidator() : base()
    {
    }
}

public sealed class GetAllJobHandler : IRequestHandler<GetAllQuery<JobViewModel>, ListDataPagination<JobViewModel>>
{
    private readonly IJobRepository _repository;
    private readonly ILogger<GetAllJobHandler> _logger;
    private readonly IValidator<GetAllQuery<JobViewModel>> _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetAllJobHandler(
        IJobRepository repository,
        ILogger<GetAllJobHandler> logger,
        IValidator<GetAllQuery<JobViewModel>> validator,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ListDataPagination<JobViewModel>> Handle(GetAllQuery<JobViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todas as vagas de emprego (Página: {Page}, Tamanho: {Size}, Ordem: {OrderBy})", request.Page, request.Size, request.OrderBy ?? "Nenhum");

        try
        {
            var staff = RecruitmentRoleNames.IsRecruitmentStaff(_httpContextAccessor.HttpContext?.User);
            bool? isDeleted = staff ? request.IsDeleted : (request.IsDeleted ?? false);
            bool? isActive = staff ? request.IsActive : (request.IsActive ?? true);
            var result = await _repository.GetAllAsync(
                cancellationToken,
                request.Page,
                request.Size,
                request.OrderBy,
                isDeleted,
                isActive);
            var jobViewModels = result.Data.Select(c => c.ToViewModel()).ToList();

            _logger.LogInformation("Total de vagas de emprego encontradas: {Count}", result.TotalItems);
            return new ListDataPagination<JobViewModel>(jobViewModels, result.TotalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar todas as vagas de emprego. Query: {@Query}", request);
            throw;
        }
    }
}