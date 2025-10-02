using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Interfaces;

namespace EmpregaNet.Application.Companies.Queries;

public sealed class GetCompanyByIdHandler : IRequestHandler<GetByIdQuery<CompanyViewModel>, CompanyViewModel>
{
    private readonly ICompanyRepository _repository;
    private readonly ILogger<GetCompanyByIdHandler> _logger;

    public GetCompanyByIdHandler(ICompanyRepository repository, ILogger<GetCompanyByIdHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CompanyViewModel> Handle(GetByIdQuery<CompanyViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando empresa por ID: {Id}", request.Id);
        try
        {
            var entity = await _repository.GetByIdAsync(request.Id);

            if (entity is null)
            {
                _logger.LogWarning("Empresa não encontrada. ID: {Id}", request.Id);
                throw new KeyNotFoundException($"Empresa com o ID {request.Id} não foi encontrada.");
            }

            var viewModel = entity.ToViewModel();
            _logger.LogInformation("Empresa encontrada: {Id}, Nome: {Nome}", request.Id, viewModel.CompanyName);
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar empresa por ID: {Id}. Request: {@Request}", request.Id, request);
            throw new Exception("Ocorreu um erro inesperado ao buscar a empresa. Por favor, tente novamente mais tarde.");
        }
    }
}