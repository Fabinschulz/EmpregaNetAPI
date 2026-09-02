using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.JobApplications.Queries;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// O que o recrutador vê na lista da vaga. O ponto é a <b>autoria do cancelamento</b>: a desistência
/// do candidato e o descarte pela empresa são dois status distintos, e a distinção tem de sobreviver
/// à leitura — é o que permite à tela dizer "cancelada pelo candidato" sem inventar a informação.
/// </summary>
/// <remarks>
/// Limitação do provider InMemory: não reproduz constraints nem semântica do PostgreSQL.
/// </remarks>
[Collection("Integration")]
public sealed class GetJobApplicationsByJobIdHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public GetJobApplicationsByJobIdHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static Job CreateJob(long companyId) => new(
        companyId: companyId,
        title: $"Auxiliar de Produção {Guid.NewGuid():N}",
        description: "Linha de montagem.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.AteUmAno,
        area: JobAreaEnum.Logistica,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        salaryMin: 2100m);

    private static GetJobApplicationsByJobIdHandler CreateSut(PostgreSqlContext context)
    {
        var employerAccess = new Mock<IJobEmployerAccess>();
        employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new GetJobApplicationsByJobIdHandler(
            new JobRepository(context),
            new JobApplicationRepository(context),
            employerAccess.Object,
            NullLogger<GetJobApplicationsByJobIdHandler>.Instance);
    }

    private static async Task SeedAsync(PostgreSqlContext context, long jobId, long userId, ApplicationStatusEnum status)
    {
        var repository = new JobApplicationRepository(context);
        var application = new JobApplication(jobId, userId);
        await repository.CreateAsync(application, CancellationToken.None);

        if (status == ApplicationStatusEnum.CanceledByCandidate)
        {
            application.CancelByCandidate();
            await repository.UpdateAsync(application, CancellationToken.None);
        }
        else if (status != ApplicationStatusEnum.Pending)
        {
            application.ChangeStatus(status);
            await repository.UpdateAsync(application, CancellationToken.None);
        }
    }

    [Fact]
    public async Task Handle_VagaComOsDoisCancelamentos_DeveDistinguirAAutoria()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7401);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        await SeedAsync(context, job.Id, userId: 7411, ApplicationStatusEnum.CanceledByCandidate);
        await SeedAsync(context, job.Id, userId: 7412, ApplicationStatusEnum.Canceled);
        await SeedAsync(context, job.Id, userId: 7413, ApplicationStatusEnum.Pending);

        var result = await CreateSut(context)
            .Handle(new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, null, null), CancellationToken.None);

        result.Data.Should().HaveCount(3);
        result.Data.Should().ContainSingle(a => a.Candidate.Id == 7411)
            .Which.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        result.Data.Should().ContainSingle(a => a.Candidate.Id == 7412)
            .Which.Status.Should().Be(ApplicationStatusEnum.Canceled);
    }

    [Fact]
    public async Task Handle_FiltroCanceladaPeloCandidato_NaoDeveTrazerOCancelamentoDaEmpresa()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var job = CreateJob(companyId: 7402);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        await SeedAsync(context, job.Id, userId: 7421, ApplicationStatusEnum.CanceledByCandidate);
        await SeedAsync(context, job.Id, userId: 7422, ApplicationStatusEnum.Canceled);

        var result = await CreateSut(context).Handle(
            new GetJobApplicationsByJobIdQuery(job.Id, 1, 100, "CanceledByCandidate", null),
            CancellationToken.None);

        result.Data.Should().ContainSingle();
        result.Data[0].Candidate.Id.Should().Be(7421);
    }
}
