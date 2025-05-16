using MediatR;

namespace EmpregaNet.Application.Commands.Base;

public sealed record UpdateCommand<TEntity, TKey>(long Id, TEntity obj) : IRequest<TEntity> where TEntity : class;
