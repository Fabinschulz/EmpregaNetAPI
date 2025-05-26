using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Interface
{
    public interface IApplicationDbContext
    {
        DbSet<Company> Company { get; }
        DbSet<Job> Job { get; }
        DbSet<JobApplication> JobApplication { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
