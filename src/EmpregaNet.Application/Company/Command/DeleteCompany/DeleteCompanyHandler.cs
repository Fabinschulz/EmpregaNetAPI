using EmpregaNet.Application.Common.Command;
using Mediator.Interfaces;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Common.Exceptions;
using EmpregaNet.Domain.Enums;
using Elastic.Apm.Api;

namespace EmpregaNet.Application.Companies.Command
{
    public sealed class DeleteCompanyHandler : IRequestHandler<DeleteCommand, bool>
    {
        private readonly ICompanyRepository _repository;
        private readonly ILogger<DeleteCompanyHandler> _logger;

        public DeleteCompanyHandler(
            ICompanyRepository repository,
            ILogger<DeleteCompanyHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando remoção da funcionalidade com ID: {Id}", request.Id);

            try
            {
                var company = await _repository.GetByIdAsync(request.Id);

                if (company == null)
                {
                    _logger.LogWarning("Tentativa de remover empresa inexistente com ID: {Id}", request.Id);
                    throw new ValidationAppException(
                        nameof(request.Id),
                        $"Empresa com ID '{request.Id}' não encontrada.",
                        DomainErrorEnum.RESOURCE_ID_NOT_FOUND);
                }

                await _repository.DeleteAsync(request.Id);
                _logger.LogInformation("Funcionalidade removida com sucesso. ID: {Id}", request.Id);

                return true;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Funcionalidade não encontrada para remoção: {Message}. Request: {@Request}", ex.Message, request);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao remover funcionalidade (ID: {Id}). Request: {@Request}", request.Id, request);
                throw new Exception("Ocorreu um erro inesperado ao remover a funcionalidade. Por favor, tente novamente mais tarde.");
            }
        }
    }
}