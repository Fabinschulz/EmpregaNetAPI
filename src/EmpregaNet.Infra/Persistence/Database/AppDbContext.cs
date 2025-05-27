using EmpregaNet.Domain;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Interface;
using EmpregaNet.Infra.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<long>, long>(options), IApplicationDbContext
    {
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<Address> Addresses => Set<Address>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.AddIdentityUserExtensions();
            //ApplyConfigurationsFromAssembly: aplica TODAS as configurações no assembly. (FluentAPI)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var dateTimeUtcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = dateTimeUtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = dateTimeUtcNow;
                        break;
                    case EntityState.Deleted:
                        entry.Entity.DeletedAt = dateTimeUtcNow;
                        entry.Entity.IsDeleted = true;
                        entry.State = EntityState.Modified;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
