using EmpregaNet.Application.Common.Command;
using EmpregaNet.Domain;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmpregaNet.Application.Common.Base;

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

public class UpdateCommandHandler<TRequest, TResponse>
    : IRequestHandler<UpdateCommand<TRequest, TResponse>, TResponse>
    where TRequest : class
    where TResponse : BaseEntity
{
    private readonly IBaseRepository<TResponse> _repository;
    protected readonly IValidator<UpdateCommand<TRequest, TResponse>> _validator;
    private readonly ILogger<UpdateCommandHandler<TRequest, TResponse>> _logger;

    public UpdateCommandHandler(
        IValidator<UpdateCommand<TRequest, TResponse>> validator,
        IBaseRepository<TResponse> repository,
        ILogger<UpdateCommandHandler<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _repository = repository;
        _validator = validator;
    }

    public async Task<TResponse> Handle(UpdateCommand<TRequest, TResponse> request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando a entidade {EntityName} com ID: {Id}", typeof(TResponse).Name, request.Id);

        await ValidateRequest(request, cancellationToken);
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entidade com ID {request.Id} não encontrada.");
        }

        // Mapear as propriedades de TRequest para TResponse
        // Aqui você pode usar um mapeador como AutoMapper ou fazer manualmente
        // Exemplo: Mapper.Map(request.entity, entity);

        var updatedEntity = await _repository.UpdateAsync(entity);
        _logger.LogInformation("{EntityName} atualizada com sucesso. ID: {Id}", typeof(TResponse).Name, updatedEntity.Id);
        return updatedEntity;
    }

    protected virtual async Task ValidateRequest(UpdateCommand<TRequest, TResponse> request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}

public sealed class GetAllHandler<TEntity> : IRequestHandler<GetAllQuery<TEntity>, ListDataPagination<TEntity>>
    where TEntity : BaseEntity
{
    private readonly IBaseRepository<TEntity> _repository;

    public GetAllHandler(IBaseRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public async Task<ListDataPagination<TEntity>> Handle(GetAllQuery<TEntity> request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(request.Page, request.Size, request.OrderBy);
    }
}

public sealed class GetByIdHandler<TEntity> : IRequestHandler<GetByIdQuery<TEntity>, TEntity>
    where TEntity : BaseEntity
{
    private readonly IBaseRepository<TEntity> _repository;

    public GetByIdHandler(IBaseRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public async Task<TEntity> Handle(GetByIdQuery<TEntity> request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id) ??
            throw new KeyNotFoundException($"{typeof(TEntity).Name} com ID {request.Id} não encontrado");
    }
}

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