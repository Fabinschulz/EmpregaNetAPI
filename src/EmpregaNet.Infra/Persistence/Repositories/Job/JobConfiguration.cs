using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories;

internal class JobConfiguration : IEntityTypeConfiguration<Job>
{
       public void Configure(EntityTypeBuilder<Job> builder)
       {
              builder.ToTable("Jobs");

              builder.HasKey(x => x.Id);

              builder.Property(x => x.Title)
                     .IsRequired()
                     .HasMaxLength(150);

              builder.Property(x => x.Description)
                     .IsRequired()
                     .HasMaxLength(2000);

              builder.Property(x => x.Salary)
                     .HasPrecision(10, 2);

              builder.Property(x => x.JobType)
                     .IsRequired();

              builder.Property(x => x.PublishedAt)
                     .IsRequired();

              builder.Property(x => x.IsActive)
                     .IsRequired();

              builder.Property(x => x.CompanyId)
                     .IsRequired();

              builder.HasIndex(x => x.CompanyId)
                     .HasDatabaseName("IX_Jobs_CompanyId");

              builder.HasIndex(x => x.IsActive)
                     .HasDatabaseName("IX_Jobs_IsActive");
       }
}

