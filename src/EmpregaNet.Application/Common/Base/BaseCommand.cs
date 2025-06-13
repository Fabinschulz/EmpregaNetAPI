using EmpregaNet.Domain.Common;
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
/// Command para Exclusão: TEntity é a entidade ou DTO de saída.
/// </summary>
/// <param name="Id"></param>
public sealed record DeleteCommand(long Id) : IRequest<bool>;

/// <summary>
/// Query para Obter por ID: TResponse é a entidade ou DTO de saída.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <param name="Id"></param>
public sealed record GetByIdQuery<TResponse>(long Id) : IRequest<TResponse> where TResponse : class;

/// <summary>
/// Query para Obter Todos: TResponse é a entidade ou DTO de saída na paginação.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <param name="Page"></param>
/// <param name="Size"></param>
/// <param name="OrderBy"></param>
public sealed record GetAllQuery<TResponse>(int Page, int Size, string? OrderBy) : IRequest<ListDataPagination<TResponse>> where TResponse : class;