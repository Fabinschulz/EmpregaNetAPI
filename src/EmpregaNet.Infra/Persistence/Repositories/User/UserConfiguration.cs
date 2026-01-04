using EmpregaNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.OwnsOne(c => c.Address, a =>
            {
                a.Property(p => p.Street).HasColumnName("Street").IsRequired().HasMaxLength(200);
                a.Property(p => p.Number).HasColumnName("Number").IsRequired().HasMaxLength(20);
                a.Property(p => p.Complement).HasColumnName("Complement").HasMaxLength(100);
                a.Property(p => p.Neighborhood).HasColumnName("Neighborhood").IsRequired().HasMaxLength(100);
                a.Property(p => p.City).HasColumnName("City").IsRequired().HasMaxLength(100);
                a.Property(p => p.State).HasColumnName("State").IsRequired().HasMaxLength(50);
                a.Property(p => p.ZipCode).HasColumnName("ZipCode").IsRequired().HasMaxLength(20);
            });

            builder.HasIndex(x => x.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(x => x.UserName)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_UserName");

            builder.HasIndex(x => x.PhoneNumber)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_PhoneNumber");

            builder.HasIndex(x => x.Id)
                   .HasDatabaseName("IX_Users_Id");
        }
    }

    internal static class IdentityUserExtensions
    {
        public static void AddIdentityUserExtensions(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(b =>
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
