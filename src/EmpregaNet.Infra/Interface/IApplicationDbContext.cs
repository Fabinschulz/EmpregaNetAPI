using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Interface
{
    public interface IApplicationDbContext
    {
        DbSet<Company> Companies { get; }
        DbSet<Job> Jobs { get; }
        DbSet<JobApplication> JobApplications { get; }
        DbSet<Address> Addresses { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
