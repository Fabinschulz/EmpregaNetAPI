using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Jobs.Queries;

public sealed class GetJobByIdHandler : IRequestHandler<GetByIdQuery<JobViewModel>, JobViewModel>
{
    private readonly IJobRepository _repository;
    private readonly ILogger<GetJobByIdHandler> _logger;

    public GetJobByIdHandler(IJobRepository repository, ILogger<GetJobByIdHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JobViewModel> Handle(GetByIdQuery<JobViewModel> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando vaga de emprego por ID: {Id}", request.Id);
        try
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                _logger.LogWarning("Vaga de emprego com ID {Id} não encontrada.", request.Id);
                throw new ValidationAppException(
                            nameof(request.Id),
                            $"Vaga de emprego com ID '{request.Id}' não encontrada.",
                            DomainErrorEnum.RECORD_NOT_EXISTS_OR_MISSING_PERMISSION);
            }

            var viewModel = entity.ToViewModel();
            _logger.LogInformation("Vaga de emprego encontrada: {Id}, Título: {Title}", request.Id, viewModel.Title);
            return viewModel;
        }
        catch (ValidationAppException ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar vaga de emprego por ID: {Message}. ID: {Id}", ex.Message, request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar vaga de emprego por ID: {Id}. Request: {@Request}", request.Id, request);
            throw new Exception("Ocorreu um erro inesperado ao buscar a vaga de emprego. Por favor, tente novamente mais tarde.", ex);
        }
    }
}