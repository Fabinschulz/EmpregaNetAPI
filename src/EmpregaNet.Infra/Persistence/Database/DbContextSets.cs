using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Interface;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OAuth.Domain.Entities;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class DbContextSets<TContext> : IdentityDbContext<User, Role, long>, IApplicationDbContext where TContext : DbContext
    {
        public DbContextSets(DbContextOptions<TContext> options) : base(options) { }


        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<Address> Addresses => Set<Address>();

    }
}
