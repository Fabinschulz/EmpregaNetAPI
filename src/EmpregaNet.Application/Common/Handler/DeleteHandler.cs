using EmpregaNet.Application.Common.Command;
using EmpregaNet.Domain;
using EmpregaNet.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Common.Handler
{
    public sealed class DeleteHandler<TEntity> : IRequestHandler<DeleteCommand<TEntity>>
     where TEntity : BaseEntity
    {
        private readonly IBaseRepository<TEntity> _repository;
        private readonly ILogger<DeleteHandler<TEntity>> _logger;

        public DeleteHandler(
            IBaseRepository<TEntity> repository,
            ILogger<DeleteHandler<TEntity>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(DeleteCommand<TEntity> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Removendo a entidade {EntityName} com o ID: {Id}",
                typeof(TEntity).Name, request.Id);

            await _repository.DeleteAsync(request.Id);
            _logger.LogInformation("{EntityName} removido com sucesso. ID: {Id}", typeof(TEntity).Name, request.Id);
        }
    }
}