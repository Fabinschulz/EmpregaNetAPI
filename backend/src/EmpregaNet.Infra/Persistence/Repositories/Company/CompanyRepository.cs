using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(PostgreSqlContext context) : base(context)
    {
    }

    public async Task<ListDataPagination<Company>> GetAllAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy,
        bool? isDeleted,
        string? search = null)
    {
        var query = _context.Companies.AsNoTracking();
        if (isDeleted.HasValue)
            query = query.Where(c => c.IsDeleted == isDeleted.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.CompanyName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.RegistrationNumber.ToLower().Contains(term));
        }
        query = ApplyCompanyOrderBy(query, orderBy);
        return await query.ToPaginatedListAsync(page, size, cancellationToken);
    }

    private static IQueryable<Company> ApplyCompanyOrderBy(IQueryable<Company> query, string? orderBy)
    {
        return orderBy switch
        {
            "createdAt_ASC" => query.OrderBy(x => x.CreatedAt),
            "createdAt_DESC" => query.OrderByDescending(x => x.CreatedAt),
            "updatedAt_ASC" => query.OrderBy(x => x.UpdatedAt),
            "updatedAt_DESC" => query.OrderByDescending(x => x.UpdatedAt),
            "id_ASC" => query.OrderBy(x => x.Id),
            "id_DESC" => query.OrderByDescending(x => x.Id),
            _ => query.OrderByDescending(x => x.CreatedAt),
        };
    }

    public async Task<Company?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _context.Companies
                             .AsNoTracking()
                             .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);
    }

    public async Task<bool> ExistsByCnpjAsync(string cnpj)
    {
        return await _context.Companies
                             .AsNoTracking()
                             .AnyAsync(c => c.RegistrationNumber == cnpj);
    }
}
