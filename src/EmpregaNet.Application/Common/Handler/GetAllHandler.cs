using EmpregaNet.Application.Common.Command;
using EmpregaNet.Domain;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using MediatR;

namespace EmpregaNet.Application.Common.Handler
{
    public class GetAllHandler<TEntity> : IRequestHandler<GetAllQuery<TEntity>, ListDataPagination<TEntity>>
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

    public class GetByIdHandler<TEntity> : IRequestHandler<GetByIdQuery<TEntity>, TEntity>
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
}