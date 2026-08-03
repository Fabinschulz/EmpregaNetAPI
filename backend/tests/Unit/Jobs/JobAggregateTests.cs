using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Domain.Jobs;

/// <summary>
/// Invariantes do agregado <see cref="Job"/> — as regras que impedem o registo de contradizer o
/// que a UI mostra.
/// </summary>
public sealed class JobAggregateTests
{
    private static Job CreateJob(
        decimal? salaryMin = 2300m,
        decimal? salaryMax = 2800m,
        bool salaryDisclosed = true,
        IEnumerable<string>? requirements = null,
        IEnumerable<string>? benefits = null)
        => new(
            companyId: 1,
            title: "Operador(a) de Empilhadeira",
            description: "Movimentação de cargas.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.SegundoTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            salaryMin: salaryMin,
            salaryMax: salaryMax,
            salaryDisclosed: salaryDisclosed,
            requirements: requirements,
            benefits: benefits);

    [Fact]
    public void Construtor_DeveNascerAtivaEComDataDePublicacao()
    {
        var job = CreateJob();

        job.IsActive.Should().BeTrue();
        job.PublishedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // Manter valores num campo que a UI não mostra faria a vaga aparecer em filtros por salário
    // sem exibir o valor que a fez aparecer.
    [Fact]
    public void SalarioNaoDivulgado_DeveZerarAFaixa()
    {
        var job = CreateJob(salaryMin: 2300m, salaryMax: 2800m, salaryDisclosed: false);

        job.SalaryDisclosed.Should().BeFalse();
        job.SalaryMin.Should().BeNull();
        job.SalaryMax.Should().BeNull();
    }

    [Fact]
    public void FaixaSalarialInvertida_DeveSerCorrigidaEmVezDeRejeitada()
    {
        var job = CreateJob(salaryMin: 3000m, salaryMax: 2000m);

        job.SalaryMin.Should().Be(2000m);
        job.SalaryMax.Should().Be(3000m);
    }

    [Fact]
    public void Requisitos_DevemSerNormalizadosSemDuplicadosNemEspacos()
    {
        var job = CreateJob(requirements: ["  Empilhadeira ", "EMPILHADEIRA", "", "  ", "CNH D"]);

        job.Requirements.Should().BeEquivalentTo(["Empilhadeira", "CNH D"]);
    }

    [Fact]
    public void Beneficios_DevemSerNormalizados()
    {
        var job = CreateJob(benefits: ["Fretado", "fretado", "Cesta Básica"]);

        job.Benefits.Should().BeEquivalentTo(["Fretado", "Cesta Básica"]);
    }

    // A coleção era `List<string>` com `private set`: protegia a reatribuição mas deixava
    // `((ICollection<string>)job.Requirements).Add(...)` corromper o agregado em silêncio —
    // `IReadOnlyList<T>` é garantia de compilação, não de runtime. Com a vista somente-leitura
    // o cast continua compilando (ReadOnlyCollection implementa ICollection), mas a mutação
    // falha alto em vez de passar despercebida.
    [Fact]
    public void Requisitos_NaoDevemAceitarMutacaoPorCast()
    {
        var job = CreateJob(requirements: ["Empilhadeira"]);

        var mutate = () => ((ICollection<string>)job.Requirements).Add("Requisito injetado");

        mutate.Should().Throw<NotSupportedException>();
        job.Requirements.Should().BeEquivalentTo(["Empilhadeira"]);
    }

    [Fact]
    public void Beneficios_NaoDevemAceitarMutacaoPorCast()
    {
        var job = CreateJob(benefits: ["Fretado"]);

        var mutate = () => ((ICollection<string>)job.Benefits).Clear();

        mutate.Should().Throw<NotSupportedException>();
        job.Benefits.Should().BeEquivalentTo(["Fretado"]);
    }

    [Fact]
    public void UpdateJob_DeveSubstituirAsColecoesEmVezDeAcumular()
    {
        var job = CreateJob(requirements: ["Empilhadeira", "CNH D"]);

        job.UpdateJob(
            title: job.Title,
            description: job.Description,
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.TerceiroTurno,
            experienceLevel: ExperienceLevelEnum.SemExperiencia,
            area: JobAreaEnum.Producao,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            requirements: ["Ensino Médio completo"]);

        job.Requirements.Should().BeEquivalentTo(["Ensino Médio completo"]);
        job.WorkShift.Should().Be(WorkShiftEnum.TerceiroTurno);
        job.ExperienceLevel.Should().Be(ExperienceLevelEnum.SemExperiencia);
        job.Area.Should().Be(JobAreaEnum.Producao);
    }

    [Fact]
    public void UpdateJob_NaoDeveReabrirVagaEncerrada()
    {
        var job = CreateJob();
        job.Close();

        job.UpdateJob(
            title: job.Title,
            description: job.Description,
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.SegundoTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = "Extrema", State = UF.MG });

        job.IsActive.Should().BeFalse("editar uma vaga encerrada não é o mesmo que reabri-la");
    }

    [Fact]
    public void VagaAfirmativa_DeveSerPreservada()
    {
        var job = new Job(
            companyId: 1,
            title: "Auxiliar Administrativo",
            description: "Rotinas de apoio.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.Administrativo,
            experienceLevel: ExperienceLevelEnum.SemExperiencia,
            area: JobAreaEnum.Administrativo,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            isPcdFriendly: true);

        job.IsPcdFriendly.Should().BeTrue();
    }
}
