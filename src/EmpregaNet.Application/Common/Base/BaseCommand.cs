using Mediator.Interfaces;

namespace EmpregaNet.Application.Common.Command;

/// <summary>
/// Command para Criação: TRequest é o DTO de entrada.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <param name="entity"></param>
public sealed record CreateCommand<TRequest>(TRequest entity) : IRequest<long> where TRequest : class;

/// <summary>
/// Command para Atualização: TRequest é o DTO de entrada, TResponse é a entidade ou DTO de saída.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="entity"></param>
/// <param name="Id"></param>
public sealed record UpdateCommand<TRequest, TResponse>(long Id, TRequest entity) : IRequest<TResponse>
    where TRequest : class
    where TResponse : class;

/// <summary>
/// Command para Exclusão: TDto é a entidade ou DTO de saída.
/// </summary>
/// <param name="Id"></param>
/// <typeparam name="TDto">
/// TDto é o tipo de dado que será excluído, geralmente um DTO ou entidade.
/// </typeparam>
public sealed record DeleteCommand<TDto>(long Id) : IRequest<bool> where TDto : class;