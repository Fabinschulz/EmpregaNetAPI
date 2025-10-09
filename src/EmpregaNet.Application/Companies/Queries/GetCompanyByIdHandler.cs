using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Enums;

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
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                _logger.LogWarning("Empresa com ID {Id} não encontrada.", request.Id);
                throw new ValidationAppException(
                            nameof(request.Id),
                            $"Empresa com ID '{request.Id}' não encontrada.",
                            DomainErrorEnum.RECORD_NOT_EXISTS_OR_MISSING_PERMISSION);
            }

            var viewModel = entity.ToViewModel();
            _logger.LogInformation("Empresa encontrada: {Id}, Nome: {Nome}", request.Id, viewModel.CompanyName);
            return viewModel;
        }
        catch (ValidationAppException ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar empresa por ID: {Message}. ID: {Id}", ex.Message, request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar empresa por ID: {Id}. Request: {@Request}", request.Id, request);
            throw new Exception("Ocorreu um erro inesperado ao buscar a empresa. Por favor, tente novamente mais tarde.", ex);
        }
    }
}