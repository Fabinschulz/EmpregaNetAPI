using EmpregaNet.Domain.Common;
using EmpregaNet.Infra.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Infra.Persistence.Database
{
    public class PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : DbContextSets<PostgreSqlContext>(options)
    {
        public async Task MigrateAsync() => await base.Database.MigrateAsync();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.AddIdentityUserExtensions();
            //ApplyConfigurationsFromAssembly: aplica TODAS as configurações no assembly. (FluentAPI)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgreSqlContext).Assembly);
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
