using EmpregaNet.Domain.Common;
using MediatR;

namespace EmpregaNet.Application.Commands.Base;

public sealed record GetByIdCommand<TEntity>(long Id) : IRequest<TEntity> where TEntity : class;
public sealed record GetAllCommand<TEntity>(int Page, int Size) : IRequest<ListDataPagination<TEntity>> where TEntity : class;


