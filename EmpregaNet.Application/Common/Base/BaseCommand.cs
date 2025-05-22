using EmpregaNet.Domain.Common;
using MediatR;

namespace EmpregaNet.Application.Common.Command;

public sealed record CreateCommand<TRequest, TResponse>(TRequest entity) : IRequest<TResponse> where TResponse : class;
public sealed record UpdateCommand<TRequest, TResponse>(long Id, TRequest entity) : IRequest<TResponse> where TResponse : class;
public sealed record DeleteCommand<TResponse>(long Id) : IRequest where TResponse : class;
public sealed record GetByIdQuery<TResponse>(long Id) : IRequest<TResponse> where TResponse : class;
public sealed record GetAllQuery<TResponse>(int Page, int Size, string? OrderBy) : IRequest<ListDataPagination<TResponse>> where TResponse : class;