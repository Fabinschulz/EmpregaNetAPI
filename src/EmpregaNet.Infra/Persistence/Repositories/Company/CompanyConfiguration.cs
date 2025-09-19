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
