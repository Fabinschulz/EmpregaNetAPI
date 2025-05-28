using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasMany(v => v.Applications)
                   .WithOne(e => e.User)
                   .HasForeignKey(v => v.UserId);

            builder.HasIndex(x => x.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(x => x.UserName)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_UserName");

            builder.HasIndex(x => x.PhoneNumber)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_PhoneNumber");
        }
    }

    public static class IdentityUserExtensions
    {
        public static void AddIdentityUserExtensions(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole<long>>(b =>
            {
                b.ToTable("Roles");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();

            });

            modelBuilder.Entity<IdentityRoleClaim<long>>(b =>
            {
                b.ToTable("RoleClaims");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<IdentityUserClaim<long>>(b =>
            {
                b.ToTable("UserClaims");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<IdentityUserLogin<long>>(b =>
            {
                b.ToTable("UserLogins");
            });

            modelBuilder.Entity<IdentityUserRole<long>>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(ur => new { ur.UserId, ur.RoleId });
            });

            modelBuilder.Entity<IdentityUserToken<long>>(b =>
            {
                b.ToTable("UserTokens");
            });
        }
    }

}
