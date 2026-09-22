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

    // ----------------------------------------------------------------------------------------
    // Posições e encerramento
    // ----------------------------------------------------------------------------------------

    private static Job CreateJobWithPositions(int positions) => new(
        companyId: 1,
        title: "Auxiliar de Produção",
        description: "Linha de montagem.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.SemExperiencia,
        area: JobAreaEnum.Producao,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        positions: positions);

    [Fact]
    public void VagaNova_DeveNascerComTodasAsPosicoesDisponiveis()
    {
        var job = CreateJobWithPositions(3);

        job.Positions.Should().Be(3);
        job.FilledPositions.Should().Be(0);
        job.AvailablePositions.Should().Be(3);
        job.Status.Should().Be(JobStatusEnum.Active);
        job.ClosedAt.Should().BeNull();
        job.ClosureReason.Should().BeNull();
        job.IsOpenForApplications.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Job.MaxPositions + 1)]
    public void TotalDePosicoesForaDoIntervalo_DeveSerRejeitado(int positions)
    {
        var act = () => CreateJobWithPositions(positions);

        act.Should().Throw<InvalidOperationException>();
    }

    // Preencher sem encher não encerra: a vaga continua a receber candidaturas para as que faltam.
    [Fact]
    public void PreencherPosicaoComSaldo_NaoDeveEncerrarAVaga()
    {
        var job = CreateJobWithPositions(2);

        var closed = job.FillPosition();

        closed.Should().BeFalse();
        job.FilledPositions.Should().Be(1);
        job.AvailablePositions.Should().Be(1);
        job.IsActive.Should().BeTrue();
        job.Status.Should().Be(JobStatusEnum.Active);
    }

    [Fact]
    public void PreencherAUltimaPosicao_DeveEncerrarAVagaPorPreenchimento()
    {
        var job = CreateJobWithPositions(2);
        job.FillPosition();

        var closed = job.FillPosition();

        closed.Should().BeTrue();
        job.AvailablePositions.Should().Be(0);
        job.IsActive.Should().BeFalse();
        job.ClosureReason.Should().Be(JobClosureReasonEnum.Fulfilled);
        job.Status.Should().Be(JobStatusEnum.ClosedByFulfillment);
        job.ClosedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        job.IsOpenForApplications.Should().BeFalse();
    }

    [Fact]
    public void PreencherPosicaoNumaVagaCheia_DeveSerRejeitado()
    {
        var job = CreateJobWithPositions(1);
        job.FillPosition();

        var act = () => job.FillPosition();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*encerrada*", "encher a última posição já encerrou a vaga");
    }

    // A vaga encerrada é terminal: libertar a posição mantém o número verdadeiro sem a ressuscitar.
    [Fact]
    public void LiberarPosicao_DeveDevolverOSaldoSemReabrirAVaga()
    {
        var job = CreateJobWithPositions(1);
        job.FillPosition();

        job.ReleasePosition();

        job.FilledPositions.Should().Be(0);
        job.AvailablePositions.Should().Be(1);
        job.IsActive.Should().BeFalse();
        job.Status.Should().Be(JobStatusEnum.ClosedByFulfillment);
    }

    [Fact]
    public void LiberarPosicaoSemNenhumaPreenchida_DeveSerRejeitado()
    {
        var job = CreateJobWithPositions(2);

        var act = job.ReleasePosition;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EncerramentoManual_DeveRegistarDataEMotivo()
    {
        var job = CreateJobWithPositions(3);

        job.Close();

        job.IsActive.Should().BeFalse();
        job.ClosureReason.Should().Be(JobClosureReasonEnum.Manual);
        job.Status.Should().Be(JobStatusEnum.ClosedManually);
        job.ClosedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EncerrarUmaVagaJaEncerrada_DeveSerRejeitado()
    {
        var job = CreateJobWithPositions(3);
        job.Close();

        var act = job.Close;

        act.Should().Throw<InvalidOperationException>();
    }

    // Reduzir o total abaixo do preenchido deixaria aprovados sem posição correspondente.
    [Fact]
    public void ReduzirTotalAbaixoDoPreenchido_DeveSerRejeitado()
    {
        var job = CreateJobWithPositions(3);
        job.FillPosition();
        job.FillPosition();

        var act = () => job.UpdateJob(
            title: "Auxiliar de Produção",
            description: "Linha de montagem.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.PrimeiroTurno,
            experienceLevel: ExperienceLevelEnum.SemExperiencia,
            area: JobAreaEnum.Producao,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            positions: 1);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*2 posição(ões) preenchida(s)*");
    }

    [Fact]
    public void AumentarOTotalDePosicoes_DeveSerPermitido()
    {
        var job = CreateJobWithPositions(2);
        job.FillPosition();

        job.UpdateJob(
            title: "Auxiliar de Produção",
            description: "Linha de montagem.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.PrimeiroTurno,
            experienceLevel: ExperienceLevelEnum.SemExperiencia,
            area: JobAreaEnum.Producao,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            positions: 5);

        job.Positions.Should().Be(5);
        job.FilledPositions.Should().Be(1);
        job.AvailablePositions.Should().Be(4);
    }

    // Subir o total numa vaga encerrada produziria "encerrada por preenchimento, com 2 vagas livres" —
    // contradição que o encerramento terminal nunca resolveria.
    [Theory]
    [InlineData(5)]
    [InlineData(1)]
    public void AlterarOTotalDePosicoesNumaVagaEncerrada_DeveSerRejeitado(int novoTotal)
    {
        var job = CreateJobWithPositions(3);
        job.Close();

        var act = () => UpdateWithPositions(job, novoTotal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*vaga encerrada*");
    }

    // A recusa é de **alterar** o total, não de editar a vaga: corrigir o título de uma vaga
    // encerrada continua a ser uma operação legítima.
    [Fact]
    public void EditarUmaVagaEncerradaMantendoOTotal_DeveSerPermitido()
    {
        var job = CreateJobWithPositions(3);
        job.Close();

        UpdateWithPositions(job, 3, title: "Auxiliar de Produção II");

        job.Title.Should().Be("Auxiliar de Produção II");
        job.Positions.Should().Be(3);
        job.IsActive.Should().BeFalse();
        job.Status.Should().Be(JobStatusEnum.ClosedManually);
    }

    private static void UpdateWithPositions(Job job, int positions, string title = "Auxiliar de Produção")
        => job.UpdateJob(
            title: title,
            description: "Linha de montagem.",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.PrimeiroTurno,
            experienceLevel: ExperienceLevelEnum.SemExperiencia,
            area: JobAreaEnum.Producao,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            positions: positions);

    // Linha anterior à capacidade: encerrada sem motivo gravado. O único encerramento que existia
    // era o manual, e é assim que tem de ser lida.
    [Fact]
    public void VagaEncerradaSemMotivoRegistado_DeveSerLidaComoEncerramentoManual()
    {
        JobStatus.Resolve(isActive: false, closureReason: null)
            .Should().Be(JobStatusEnum.ClosedManually);
    }
}
