using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(v => v.Company)
               .WithMany(e => e.Jobs)
               .HasForeignKey(v => v.CompanyId);

        builder.HasMany(v => v.Applications)
               .WithOne(e => e.Job)
               .HasForeignKey(v => v.JobId);

        builder.HasIndex(x => x.Title)
               .IsUnique()
               .HasDatabaseName("IX_Jobs_Title");
        builder.HasIndex(x => x.PublicationDate)
               .HasDatabaseName("IX_Jobs_PublicationDate");
    }

}
