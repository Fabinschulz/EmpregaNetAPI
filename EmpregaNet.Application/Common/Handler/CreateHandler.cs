using EmpregaNet.Application.Common.Command;
using EmpregaNet.Domain;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Common.Handler
{
    public class CreateHandler<TEntity> : IRequestHandler<CreateCommand<TEntity, TEntity>, TEntity>
        where TEntity : BaseEntity
    {
        protected readonly IBaseRepository<TEntity> _repository;
        protected readonly IValidator<CreateCommand<TEntity, TEntity>> _validator;
        protected readonly ILogger<CreateHandler<TEntity>> _logger;

        public CreateHandler(
            IBaseRepository<TEntity> repository,
            IValidator<CreateCommand<TEntity, TEntity>> validator,
            ILogger<CreateHandler<TEntity>> logger)
        {
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }

        public virtual async Task<TEntity> Handle(CreateCommand<TEntity, TEntity> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Criando a entidade {EntityName}:", typeof(TEntity).Name);

            await ValidateRequest(request, cancellationToken);
            var createdEntity = await _repository.CreateAsync(request.entity);

            _logger.LogInformation("{EntityName} criada com sucesso. ID: {Id}", typeof(TEntity).Name, createdEntity.Id);
            return createdEntity;
        }

        protected virtual async Task ValidateRequest(CreateCommand<TEntity, TEntity> request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

    }
}