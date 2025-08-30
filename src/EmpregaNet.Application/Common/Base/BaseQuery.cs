using EmpregaNet.Domain.Common;
using Mediator.Interfaces;

namespace EmpregaNet.Application.Common.Command;

/// <summary>
/// Query para Obter por ID: TResponse é a entidade ou DTO de saída.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <param name="Id"></param>
public sealed record GetByIdQuery<TResponse>(long Id) : IRequest<TResponse>;

/// <summary>
/// Query para Obter Todos: TResponse é a entidade ou DTO de saída na paginação.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <param name="Page"></param>
/// <param name="Size"></param>
/// <param name="OrderBy"></param>
public sealed record GetAllQuery<TResponse>(int Page, int Size, string? OrderBy) : IRequest<ListDataPagination<TResponse>>;