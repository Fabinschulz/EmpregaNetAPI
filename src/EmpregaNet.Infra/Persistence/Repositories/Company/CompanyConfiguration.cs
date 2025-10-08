using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories;

internal class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasMany(v => v.Jobs)
               .WithOne(e => e.Company)
               .HasForeignKey(v => v.CompanyId)
               .OnDelete(DeleteBehavior.SetNull);

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

        builder.HasIndex(x => x.CompanyName)
               .IsUnique()
               .HasDatabaseName("IX_Companies_Name");

        builder.HasIndex(x => x.RegistrationNumber)
               .IsUnique()
               .HasDatabaseName("IX_Companies_Cnpj");

        builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Companies_Email");
    }

}
