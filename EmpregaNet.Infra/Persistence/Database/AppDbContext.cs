
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Interface;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Database
{
    /// <summary>
    /// Represents the application's database context.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
    {
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Configures the schema needed for the context.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.UserName).IsRequired(false).HasMaxLength(80).HasColumnType("varchar(80)");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
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
