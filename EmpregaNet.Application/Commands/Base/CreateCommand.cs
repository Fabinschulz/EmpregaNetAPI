using MediatR;

namespace EmpregaNet.Application.Commands.Base;

public sealed record CreateCommand<TEntity>(TEntity obj) : IRequest<TEntity> where TEntity : class;

