using Microsoft.Extensions.Logging;
using EmpregaNet.Application.Companies.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Application.Companies.Command.Delete
{
    public sealed class DeleteCompanyHandler : IRequestHandler<DeleteCommand<CompanyViewModel>, bool>
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

        public async Task<bool> Handle(DeleteCommand<CompanyViewModel> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando remoção da empresa com ID: {Id}", request.Id);

            try
            {
                await _repository.DeleteAsync(request.Id);
                _logger.LogInformation("Empresa removida com sucesso. ID: {Id}", request.Id);
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Empresa não encontrada para remoção: {Message}. Request: {@Request}", ex.Message, request);
                throw new KeyNotFoundException("A empresa que você está tentando remover não existe ou já foi removida.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao remover empresa (ID: {Id}). Request: {@Request}", request.Id, request);
                throw new Exception("Ocorreu um erro inesperado ao remover a empresa. Por favor, tente novamente mais tarde.");
            }
        }
    }
}