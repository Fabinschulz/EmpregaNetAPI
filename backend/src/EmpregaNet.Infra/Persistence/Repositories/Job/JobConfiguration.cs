using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpregaNet.Infra.Persistence.Repositories;

/// <summary>
/// Mapeamento da vaga comum a todos os providers. A coluna gerada de busca é específica do
/// Postgres e vive em <see cref="JobSearchVector"/>, aplicada condicionalmente pelo contexto.
/// </summary>
internal class JobConfiguration : IEntityTypeConfiguration<Job>
{
       public void Configure(EntityTypeBuilder<Job> builder)
       {
              builder.ToTable("Jobs");

              builder.HasKey(x => x.Id);

              builder.Property(x => x.Title)
                     .IsRequired()
                     .HasMaxLength(150);

              builder.Property(x => x.Summary)
                     .HasMaxLength(280);

              builder.Property(x => x.Description)
                     .IsRequired()
                     .HasMaxLength(2000);

              builder.Property(x => x.SalaryMin)
                     .HasPrecision(10, 2);

              builder.Property(x => x.SalaryMax)
                     .HasPrecision(10, 2);

              builder.Property(x => x.SalaryDisclosed)
                     .IsRequired()
                     .HasDefaultValue(true);

              builder.Property(x => x.JobType)
                     .IsRequired();

              builder.Property(x => x.WorkModel)
                     .IsRequired();

              builder.Property(x => x.WorkShift)
                     .IsRequired();

              builder.Property(x => x.ExperienceLevel)
                     .IsRequired();

              builder.Property(x => x.Area)
                     .IsRequired();

              builder.Property(x => x.IsPcdFriendly)
                     .IsRequired()
                     .HasDefaultValue(false);

              // Coleções expostas como IReadOnlyList sobre campo privado: o EF escreve pelo campo.
              builder.Property(x => x.Requirements)
                     .HasField("_requirements")
                     .UsePropertyAccessMode(PropertyAccessMode.Field)
                     .IsRequired()
                     .HasColumnType("text[]")
                     .HasDefaultValueSql("'{}'::text[]");

              builder.Property(x => x.Benefits)
                     .HasField("_benefits")
                     .UsePropertyAccessMode(PropertyAccessMode.Field)
                     .IsRequired()
                     .HasColumnType("text[]")
                     .HasDefaultValueSql("'{}'::text[]");

              builder.Property(x => x.PublishedAt)
                     .IsRequired();

              builder.Property(x => x.IsActive)
                     .IsRequired();

              builder.Property(x => x.CompanyId)
                     .IsRequired();

              builder.OwnsOne(j => j.Location, location =>
              {
                     location.Property(p => p.City)
                             .HasColumnName("City")
                             .IsRequired()
                             .HasMaxLength(100);

                     location.Property(p => p.State)
                             .HasColumnName("State")
                             .IsRequired();

                     location.Property(p => p.Country)
                             .HasColumnName("Country")
                             .IsRequired()
                             .HasMaxLength(2)
                             .HasDefaultValue("BR");

                     location.HasIndex(p => new { p.State, p.City })
                             .HasDatabaseName("IX_Jobs_Location");
              });

              builder.Navigation(j => j.Location).IsRequired();

              builder.HasIndex(x => x.CompanyId)
                     .HasDatabaseName("IX_Jobs_CompanyId");

              builder.HasIndex(x => x.IsActive)
                     .HasDatabaseName("IX_Jobs_IsActive");

              // Predicado + ORDER BY do caminho padrão do feed numa varredura só.
              builder.HasIndex(x => new { x.IsDeleted, x.IsActive, x.PublishedAt })
                     .IsDescending(false, false, true)
                     .HasDatabaseName("IX_Jobs_Feed");

              builder.HasIndex(x => x.SalaryMax)
                     .IsDescending()
                     .HasDatabaseName("IX_Jobs_SalaryMax");

              builder.HasIndex(x => x.Requirements)
                     .HasMethod("gin")
                     .HasDatabaseName("IX_Jobs_Requirements");

              builder.HasIndex(x => x.Benefits)
                     .HasMethod("gin")
                     .HasDatabaseName("IX_Jobs_Benefits");

              builder.HasIndex(x => x.IsPcdFriendly)
                     .HasFilter("\"IsPcdFriendly\"")
                     .HasDatabaseName("IX_Jobs_PcdFriendly");
       }
}
