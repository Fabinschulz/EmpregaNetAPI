using EmpregaNet.Application.Common.Command;
using EmpregaNet.Domain;
using EmpregaNet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

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