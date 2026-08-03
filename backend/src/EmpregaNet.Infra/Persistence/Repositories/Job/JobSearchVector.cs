using EmpregaNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace EmpregaNet.Infra.Persistence.Repositories;

/// <summary>
/// Coluna gerada de busca full-text da vaga - mapeamento exclusivo do Postgres.
/// </summary>
internal static class JobSearchVector
{
    internal const string PropertyName = "SearchVector";
    internal const string SearchConfiguration = "pt_unaccent";
    internal const string IndexName = "IX_Jobs_SearchVector";

    /// <summary>
    /// Função criada na migration para juntar um <c>text[]</c> num texto único.
    /// </summary>
    internal const string ArrayToTextFunction = "empreganet_array_to_text";

    /// <summary>
    /// Os pesos são o que dá sentido à ordenação por relevância: o título pesa mais que a
    /// descrição, então buscar "xpto" prioriza a vaga de XPTO sobre a que só a cita no texto.
    /// </summary>
    private const string ComputedSql = $"""
        setweight(to_tsvector('{SearchConfiguration}', coalesce("Title", '')), 'A') ||
        setweight(to_tsvector('{SearchConfiguration}', coalesce("Summary", '')), 'B') ||
        setweight(to_tsvector('{SearchConfiguration}', {ArrayToTextFunction}("Requirements")), 'B') ||
        setweight(to_tsvector('{SearchConfiguration}', {ArrayToTextFunction}("Benefits")), 'C') ||
        setweight(to_tsvector('{SearchConfiguration}', coalesce("Description", '')), 'D')
        """;

    internal static void Apply(ModelBuilder modelBuilder)
    {
        var job = modelBuilder.Entity<Job>();

        job.Property<NpgsqlTsVector>(PropertyName)
           .HasColumnType("tsvector")
           .HasComputedColumnSql(ComputedSql, stored: true);

        job.HasIndex(PropertyName)
           .HasMethod("gin")
           .HasDatabaseName(IndexName);
    }
}
