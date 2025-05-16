using EmpregaNet.Domain;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Application.Services
{
    public class BaseService<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            var existing = await _repository.GetByIdAsync(entity.Id);
            if (existing != null)
                throw new InvalidOperationException($"{typeof(T).Name} com ID {entity.Id} já existe");

            return await _repository.CreateAsync(entity);
        }

        public virtual async Task<bool> DeleteAsync(T entity)
        {
            await _repository.DeleteAsync(entity);
            return true;
        }

        public virtual async Task<ListDataPagination<T>> GetAllAsync(int page, int size, string? orderBy = null)
        {
            return await _repository.GetAllAsync(page, size, orderBy);
        }

        public virtual async Task<T?> GetByIdAsync(long id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} com ID {id} não encontrado");

            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            // Verifica se existe antes de atualizar
            await GetByIdAsync(entity.Id);
            return await _repository.UpdateAsync(entity);
        }
    }
}