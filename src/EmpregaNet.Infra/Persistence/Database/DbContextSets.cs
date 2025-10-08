using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class DbContextSets<TContext> : IdentityDbContext<User, Role, long> where TContext : DbContext
    {
        public DbContextSets(DbContextOptions<TContext> options) : base(options) { }


        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    }
}
