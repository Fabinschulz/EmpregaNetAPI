using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories;

internal class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId)
               .IsRequired();

        builder.Property(x => x.UserId)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.AppliedAt)
               .IsRequired();

        builder.HasIndex(x => new { x.JobId, x.UserId })
               .IsUnique()
               .HasDatabaseName("IX_JobApplications_JobId_UserId");

        builder.HasIndex(x => x.JobId)
               .HasDatabaseName("IX_JobApplications_JobId");
    }
}
