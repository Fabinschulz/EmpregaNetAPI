using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories
{
    /// <summary>
    /// Repositório base para operações de persistência de dados.
    ///
    /// Este repositório encapsula a lógica de acesso a dados (CRUD) para uma entidade
    /// genérica T. Ele atua como uma ponte entre a lógica de negócio e o banco de dados.
    ///
    /// Principais responsabilidades:
    /// - Gerenciar consultas (como GetById e GetAll).
    /// - Marcar entidades para criação, atualização ou exclusão.
    ///
    /// </summary>
    /// <remarks>
    /// A persistência real das mudanças no banco de dados é delegada à Unit of Work,
    /// que coordena a transação e garante que todas as operações sejam concluídas com sucesso.
    /// Isso permite que múltiplos repositórios e operações sejam agrupados em uma única
    /// transação atômica. Isso previne dados inconsistentes
    /// e segue o padrão de design para transações.
    /// </remarks>
    /// <typeparam name="T">O tipo da entidade que o repositório gerencia.</typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly PostgreSqlContext _context;

        public BaseRepository(PostgreSqlContext context)
        {
            _context = context;
        }
        public async Task<T?> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ListDataPagination<T>> GetAllAsync(int Page, int Size, string? orderBy)
        {
            var query = _context.Set<T>().AsNoTracking();

            if (!string.IsNullOrEmpty(orderBy))
            {
                query = ApplyOrderBy(query, orderBy);
            }

            var result = await query.ToPaginatedListAsync(Page, Size);
            return result;
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} com ID {id} não encontrado");

            //_context.Set<T>().Remove(entity);
            _context.Set<T>().Update(entity);
            entity.IsDeleted = true;
            return true;
        }

        private static IQueryable<T> ApplyOrderBy(IQueryable<T> query, string orderBy)
        {
            switch (orderBy)
            {
                case "createdAt_ASC":
                    return query.OrderBy(x => x.CreatedAt);
                case "createdAt_DESC":
                    return query.OrderByDescending(x => x.CreatedAt);
                case "updatedAt_ASC":
                    return query.OrderBy(x => x.UpdatedAt);
                case "updatedAt_DESC":
                    return query.OrderByDescending(x => x.UpdatedAt);
                case "id_ASC":
                    return query.OrderBy(x => x.Id);
                case "id_DESC":
                    return query.OrderByDescending(x => x.Id);
                default:
                    return query.OrderByDescending(x => x.CreatedAt);
            }
        }

    }
}