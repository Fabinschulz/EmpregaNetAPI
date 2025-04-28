
using EmpregaNet.Domain;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Interface;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options), IApplicationDbContext
    {
        public DbSet<Company> Company => Set<Company>();
        public DbSet<Job> Job => Set<Job>();
        public DbSet<JobApplication> JobApplication => Set<JobApplication>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>()
                        .HasOne(v => v.Company)
                        .WithMany(e => e.Jobs)
                        .HasForeignKey(v => v.CompanyId);

            modelBuilder.Entity<JobApplication>()
                        .HasOne(c => c.Job)
                        .WithMany(v => v.Applications)
                        .HasForeignKey(c => c.JobId);

            modelBuilder.Entity<JobApplication>()
                        .HasOne(c => c.User)
                        .WithMany(u => u.Applications)
                        .HasForeignKey(c => c.UserId);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
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
