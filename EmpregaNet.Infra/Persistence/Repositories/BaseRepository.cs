using EmpregaNet.Domain;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {

        protected readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
            return entity;
        }

        public async Task<T?> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
        }


        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} com ID {id} não encontrado");

            //_context.Set<T>().Remove(entity);
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
            return true;
        }

        public async Task<ListDataPagination<T>> GetAllAsync(int Page, int Size, string? orderBy)
        {
            var query = _context.Set<T>().AsQueryable();

            if (!string.IsNullOrEmpty(orderBy))
            {
                query = ApplyOrderBy(query, orderBy);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / Size);
            var data = await query.Skip(Page * Size).Take(Size).ToListAsync();

            return new ListDataPagination<T>(data, Page, totalPages, totalItems);
        }

        private static IQueryable<T> ApplyOrderBy(IQueryable<T> query, string orderBy)
        {
            switch (orderBy)
            {
                case "createdAt_ASC":
                    return query.OrderBy(x => x.CreatedAt);
                case "createdAt_DESC":
                    return query.OrderByDescending(x => x.CreatedAt);
                default:
                    return query.OrderByDescending(x => x.CreatedAt);
            }
        }

    }
}