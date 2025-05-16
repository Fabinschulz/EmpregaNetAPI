using MediatR;

namespace EmpregaNet.Application.Commands.Base;

public sealed record DeleteCommand<TEntity>(long Id) : IRequest where TEntity : class;
