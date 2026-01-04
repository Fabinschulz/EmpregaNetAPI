using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class JobRepository : BaseRepository<Job>, IJobRepository
{
    public JobRepository(PostgreSqlContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByTitleAndCompanyIdAsync(string title, long companyId)
    {
        return await _context.Jobs.AnyAsync(j => j.Title == title && j.CompanyId == companyId);
    }
}
