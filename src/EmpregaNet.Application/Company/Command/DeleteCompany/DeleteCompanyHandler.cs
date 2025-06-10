using EmpregaNet.Application.Common.Command;
using EmpregaNet.Application.Messages;
using EmpregaNet.Domain.Components.Mediator.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Companies.Command
{
    public sealed class DeleteCompanyHandler : IRequestHandler<DeleteCommand<bool>, bool>
    {
        private readonly ICompanyRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteCompanyHandler> _logger;

        public DeleteCompanyHandler(
            IMediator mediator,
            ICompanyRepository repository,
            ILogger<DeleteCompanyHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(DeleteCommand<bool> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando remoção da funcionalidade com ID: {Id}", request.Id);

            try
            {
                var company = await _repository.GetByIdAsync(request.Id);
                if (company == null)
                {
                    _logger.LogWarning("Tentativa de remover empresa inexistente com ID: {Id}", request.Id);
                    throw new KeyNotFoundException($"Empresa com ID '{request.Id}' não encontrada.");
                }

                await _repository.DeleteAsync(request.Id);
                _logger.LogInformation("Funcionalidade removida com sucesso. ID: {Id}", request.Id);

                await _mediator.Publish(new EntityEvent<Company>(company), cancellationToken);
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