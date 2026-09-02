using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EmpregaNet.Tests.Unit.Persistence;

/// <summary>
/// O índice único de <c>(JobId, UserId)</c> tem de ser <b>parcial</b>: N candidaturas canceladas pelo
/// candidato podem coexistir, uma activa não pode duplicar.
/// </summary>
/// <remarks>
/// <b>Porque este teste existe e o que ele não cobre.</b> A regra vive no PostgreSQL, e a suíte corre
/// sobre o provider InMemory, que <b>não</b> aplica índices nem constraints. Foi exactamente isso que
/// escondeu o defeito original: <c>ApplyToJobHandlerIntegrationTests</c> "prova" a recandidatura e
/// continuaria verde mesmo com o índice total, porque no InMemory a segunda inserção nunca colide.
///
/// <para>
/// O que se pode afirmar sem uma base real é o <b>modelo</b>: que a configuração declara o índice como
/// único e com o filtro certo, e que é dele que a migration deriva. O que fica por provar é o
/// comportamento do servidor — que o PostgreSQL aceita mesmo a segunda linha. Isso exige base real
/// (Testcontainers ou ambiente), e não existe neste projecto.
/// </para>
/// </remarks>
public sealed class JobApplicationIndexTests
{
    private static PostgreSqlContext CreateContext() =>
        new(new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseInMemoryDatabase($"model_{Guid.NewGuid():N}")
            .Options);

    [Fact]
    public void IndiceDeCandidatura_DeveSerUnicoEParcial()
    {
        using var context = CreateContext();

        var index = context.Model
            .FindEntityType(typeof(JobApplication))!
            .GetIndexes()
            .Single(i => i.GetDatabaseName() == "IX_JobApplications_JobId_UserId");

        index.IsUnique.Should().BeTrue();
        index.Properties.Select(p => p.Name).Should().Equal(nameof(JobApplication.JobId), nameof(JobApplication.UserId));
        index.GetFilter().Should().NotBeNullOrWhiteSpace(
            "sem filtro o índice é total e a recandidatura de X4 viola-o no PostgreSQL");
    }

    /// <summary>
    /// Amarra o literal do SQL ao membro do enum. O filtro é texto — o compilador não o verifica — e a
    /// coluna é <c>integer</c> sobre um enum sem valores explícitos, por isso o número no SQL é a
    /// <b>posição</b> do membro. Se alguém inserir um valor no meio do enum, este teste e o
    /// <c>ApplicationStatusEnum_OrdemDosValores_DeveSerEstavel</c> falham juntos, antes de o filtro
    /// passar a excluir o status errado.
    /// </summary>
    [Fact]
    public void FiltroDoIndice_DeveExcluirExactamenteOStatusCanceladoPeloCandidato()
    {
        using var context = CreateContext();

        var filter = context.Model
            .FindEntityType(typeof(JobApplication))!
            .GetIndexes()
            .Single(i => i.GetDatabaseName() == "IX_JobApplications_JobId_UserId")
            .GetFilter();

        var expected = $"\"Status\" <> {(int)ApplicationStatusEnum.CanceledByCandidate}";

        filter.Should().Be(expected);
    }
}
