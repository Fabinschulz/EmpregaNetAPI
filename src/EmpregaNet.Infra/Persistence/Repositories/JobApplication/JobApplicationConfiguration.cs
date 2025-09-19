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
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(v => v.Job)
               .WithMany(e => e.Applications)
               .HasForeignKey(v => v.JobId);

        builder.HasOne(v => v.User)
               .WithMany(e => e.Applications)
               .HasForeignKey(v => v.UserId);

        builder.HasIndex(x => new { x.JobId, x.UserId })
               .IsUnique()
               .HasDatabaseName("IX_JobApplications_JobId_UserId");
    }

}