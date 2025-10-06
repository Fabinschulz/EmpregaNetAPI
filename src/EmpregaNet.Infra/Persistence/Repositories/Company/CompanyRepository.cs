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

    public async Task<Company?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _context.Companies
                             .AsNoTracking()
                             .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);
    }
}
