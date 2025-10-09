using EmpregaNet.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Application.Companies.Queries;

public sealed class GetAllValidator : BasePaginatedQueryValidator<GetAllQuery<CompanyViewModel>>
{
    public GetAllValidator() : base()
    {
    }
}

public sealed class GetAllCompanyHandler : IRequestHandler<GetAllQuery<CompanyViewModel>, ListDataPagination<CompanyViewModel>>
{
    private readonly ICompanyRepository _repository;
    private readonly ILogger<GetAllCompanyHandler> _logger;
    private readonly IValidator<GetAllQuery<CompanyViewModel>> _validator;

    public GetAllCompanyHandler(ICompanyRepository repository, ILogger<GetAllCompanyHandler> logger, IValidator<GetAllQuery<CompanyViewModel>> validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<ListDataPagination<CompanyViewModel>> Handle(GetAllQuery<CompanyViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todas as empresas (Página: {Page}, Tamanho: {Size}, Ordem: {OrderBy})", request.Page, request.Size, request.OrderBy ?? "Nenhum");

        try
        {
            var result = await _repository.GetAllAsync(cancellationToken, request.Page, request.Size, request.OrderBy);
            var companyViewModels = result.Data.Select(c => c.ToViewModel()).ToList();

            _logger.LogInformation("Total de empresas encontradas: {Count}", result.TotalItems);
            return new ListDataPagination<CompanyViewModel>(companyViewModels, result.TotalItems, request.Page, request.Size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar todas as empresas. Query: {@Query}", request);
            throw;
        }
    }
}