using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Infra.Persistence.Repositories;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Integration.Handlers;

/// <summary>
/// A edição de vaga perante o estado de preenchimento: o que ela pode mudar, o que recusa, e o que
/// nunca pode sobrescrever.
/// </summary>
/// <remarks>
/// <b>Limitação do provider InMemory:</b> não há bloqueio de linha nem transacções reais, portanto
/// estes testes <b>não</b> provam a serialização entre edição e aprovação concorrentes — isso é
/// comportamento do <c>SELECT … FOR UPDATE</c> e só se verifica contra PostgreSQL. O que fica
/// provado aqui é a parte independente do provider: gravar a edição não repõe o estado de
/// encerramento nem o contador, e a recusa do agregado chega como erro de negócio.
/// </remarks>
[Collection("Integration")]
public sealed class UpdateJobHandlerIntegrationTests
{
    private readonly InMemoryIdentityFixture _fixture;

    public UpdateJobHandlerIntegrationTests(InMemoryIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    private static Company CreateCompany() => new()
    {
        CompanyName = $"Acme {Guid.NewGuid():N}",
        RegistrationNumber = $"{Random.Shared.NextInt64(10000000000000, 99999999999999)}",
        Email = "contato@acme.local",
        Phone = "1133334444",
        TypeOfActivity = TypeOfActivityEnum.Industry,
        Address = new Address
        {
            Street = "Av Paulista",
            Number = "1000",
            Neighborhood = "Bela Vista",
            City = "São Paulo",
            State = UF.SP,
            ZipCode = "01310100"
        }
    };

    private static Job CreateJob(long companyId, int positions) => new(
        companyId: companyId,
        title: $"Auxiliar de Producao {Guid.NewGuid():N}",
        description: "Linha de montagem.",
        jobType: JobTypeEnum.Clt,
        workModel: WorkModelEnum.OnSite,
        workShift: WorkShiftEnum.PrimeiroTurno,
        experienceLevel: ExperienceLevelEnum.SemExperiencia,
        area: JobAreaEnum.Producao,
        location: new JobLocation { City = "Extrema", State = UF.MG },
        positions: positions,
        salaryMin: 2200m);

    private static UpdateJobCommand CommandFor(Job job, int positions, string? title = null) => new(
        CompanyId: job.CompanyId,
        Title: title ?? job.Title,
        Description: job.Description,
        JobType: nameof(JobTypeEnum.Clt),
        WorkModel: nameof(WorkModelEnum.OnSite),
        WorkShift: nameof(WorkShiftEnum.PrimeiroTurno),
        ExperienceLevel: nameof(ExperienceLevelEnum.SemExperiencia),
        Area: nameof(JobAreaEnum.Producao),
        City: "Extrema",
        State: nameof(UF.MG),
        Positions: positions,
        SalaryMin: 2200m);

    private static UpdateJobHandler CreateSut(IServiceScope scope, PostgreSqlContext context)
    {
        var currentUser = new Mock<IHttpCurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(7300);
        currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = 7300,
                Username = "recrutador",
                Email = "recrutador@test.local",
                Roles = ["Recruiter"],
                Claims = []
            }
        });

        var employerAccess = new Mock<IJobEmployerAccess>();
        employerAccess
            .Setup(x => x.EnsureCanManageCompanyAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new UpdateJobHandler(
            new JobRepository(context),
            new CompanyRepository(context),
            NullLogger<UpdateJobHandler>.Instance,
            currentUser.Object,
            employerAccess.Object,
            scope.ServiceProvider.GetRequiredService<UserManager<User>>());
    }

    private static Task<JobViewModel> ExecuteAsync(UpdateJobHandler sut, long jobId, UpdateJobCommand command)
        => sut.Handle(new UpdateCommand<UpdateJobCommand, JobViewModel>(jobId, command), CancellationToken.None);

    private async Task<(IServiceScope scope, PostgreSqlContext context, Job job)> SeedAsync(int positions)
    {
        var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

        var company = CreateCompany();
        await new CompanyRepository(context).CreateAsync(company, CancellationToken.None);

        var job = CreateJob(company.Id, positions);
        await new JobRepository(context).CreateAsync(job, CancellationToken.None);

        return (scope, context, job);
    }

    /// <summary>
    /// O caso que motivou o bloqueio de linha: a gravação da edição escreve a linha inteira, logo
    /// não pode repor <c>IsActive</c>, <c>ClosedAt</c>, <c>ClosureReason</c> nem o contador.
    /// </summary>
    [Fact]
    public async Task Handle_VagaEncerradaPorPreenchimento_NaoDeveSerReabertaPelaEdicao()
    {
        var (scope, context, job) = await SeedAsync(positions: 1);
        using var _ = scope;

        job.FillPosition().Should().BeTrue("uma posição de uma, a vaga encerra");
        await new JobRepository(context).UpdateAsync(job, CancellationToken.None);
        context.ChangeTracker.Clear();

        await ExecuteAsync(CreateSut(scope, context), job.Id, CommandFor(job, positions: 1, title: "Título revisto"));
        context.ChangeTracker.Clear();

        var reloaded = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloaded.Title.Should().Be("Título revisto");
        reloaded.IsActive.Should().BeFalse();
        reloaded.ClosureReason.Should().Be(JobClosureReasonEnum.Fulfilled);
        reloaded.ClosedAt.Should().NotBeNull();
        reloaded.FilledPositions.Should().Be(1);
    }

    // Antes, a recusa do agregado subia como InvalidOperationException e o handler global devolvia
    // 409 "Operação inválida." sem detalhe fora de Development — o recrutador não sabia o que corrigir.
    [Fact]
    public async Task Handle_TotalAbaixoDoPreenchido_DeveDevolverErroDeNegocioComMotivo()
    {
        var (scope, context, job) = await SeedAsync(positions: 3);
        using var _ = scope;

        job.FillPosition();
        job.FillPosition();
        await new JobRepository(context).UpdateAsync(job, CancellationToken.None);
        context.ChangeTracker.Clear();

        var act = async () => await ExecuteAsync(CreateSut(scope, context), job.Id, CommandFor(job, positions: 1));

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        assertion.Which.Message.Should().Contain("2 posição(ões) preenchida(s)");
    }

    [Fact]
    public async Task Handle_AlterarTotalDeVagaEncerrada_DeveDevolverErroDeNegocio()
    {
        var (scope, context, job) = await SeedAsync(positions: 2);
        using var _ = scope;

        job.Close();
        await new JobRepository(context).UpdateAsync(job, CancellationToken.None);
        context.ChangeTracker.Clear();

        var act = async () => await ExecuteAsync(CreateSut(scope, context), job.Id, CommandFor(job, positions: 5));

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        assertion.Which.Message.Should().Contain("vaga encerrada");
    }

    [Fact]
    public async Task Handle_AumentarOTotalDeVagaActiva_DeveSerPersistido()
    {
        var (scope, context, job) = await SeedAsync(positions: 2);
        using var _ = scope;

        job.FillPosition();
        await new JobRepository(context).UpdateAsync(job, CancellationToken.None);
        context.ChangeTracker.Clear();

        var result = await ExecuteAsync(CreateSut(scope, context), job.Id, CommandFor(job, positions: 5));
        context.ChangeTracker.Clear();

        result.Positions.Should().Be(5);
        result.AvailablePositions.Should().Be(4);

        var reloaded = await context.Jobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        reloaded.Positions.Should().Be(5);
        reloaded.FilledPositions.Should().Be(1, "editar a vaga não mexe em quem já foi aprovado");
        reloaded.IsActive.Should().BeTrue();
    }
}
