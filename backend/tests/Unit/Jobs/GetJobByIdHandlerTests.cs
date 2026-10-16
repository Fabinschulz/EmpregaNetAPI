using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

/// <summary>
/// Regressão: o endpoint declarava <c>404</c> em <c>ProducesResponseType</c> e devolvia sempre
/// <c>400</c>, porque "não encontrada" era sinalizado com <c>ValidationAppException</c>. O
/// consumidor tratava vaga inexistente como erro inesperado em vez de página não encontrada, e
/// nada no código acusava a divergência.
/// </summary>
public sealed class GetJobByIdHandlerTests
{
    private readonly Mock<IJobRepository> _repository = new();
    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor
    {
        HttpContext = new DefaultHttpContext()
    };

    private GetJobByIdHandler CreateSut() =>
        new(_repository.Object, NullLogger<GetJobByIdHandler>.Instance, _httpContextAccessor);

    private static Job CreateJob(bool isActive = true, bool isDeleted = false)
    {
        var job = new Job(
            companyId: 1,
            title: "Operador(a) de Empilhadeira",
            description: "Descrição",
            jobType: JobTypeEnum.Clt,
            workModel: WorkModelEnum.OnSite,
            workShift: WorkShiftEnum.SegundoTurno,
            experienceLevel: ExperienceLevelEnum.AteUmAno,
            area: JobAreaEnum.Logistica,
            location: new JobLocation { City = "Extrema", State = UF.MG },
            salaryMin: 2300m);

        if (!isActive) job.Close();
        job.IsDeleted = isDeleted;

        return job;
    }

    private void SetupRepository(Job? job) =>
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

    [Fact]
    public async Task Handle_VagaInexistente_DeveLancarNotFoundENaoValidacao()
    {
        SetupRepository(null);

        var act = async () => await CreateSut().Handle(new GetByIdQuery<JobViewModel>(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_VagaEncerradaSemPerfilDeRecrutamento_DeveDevolverComIsActiveFalseEClosedAt()
    {
        SetupRepository(CreateJob(isActive: false));

        var result = await CreateSut().Handle(new GetByIdQuery<JobViewModel>(1), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        result.ClosedAt.Should().NotBeNull();
        result.Title.Should().Be("Operador(a) de Empilhadeira");
    }

    [Fact]
    public async Task Handle_VagaEncerradaEExcluidaSemPerfilDeRecrutamento_DeveLancarNotFound()
    {
        SetupRepository(CreateJob(isActive: false, isDeleted: true));

        var act = async () => await CreateSut().Handle(new GetByIdQuery<JobViewModel>(1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_StaffDeveVerVagaExcluida()
    {
        SetupRepository(CreateJob(isActive: false, isDeleted: true));
        _httpContextAccessor.HttpContext!.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, RecruitmentRoleNames.Admin)],
                "test"));

        var result = await CreateSut().Handle(new GetByIdQuery<JobViewModel>(1), CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_VagaExcluidaSemPerfilDeRecrutamento_DeveLancarNotFound()
    {
        SetupRepository(CreateJob(isDeleted: true));

        var act = async () => await CreateSut().Handle(new GetByIdQuery<JobViewModel>(1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // O catch genérico do handler reembrulha qualquer exceção como erro inesperado. Sem um catch
    // dedicado ao NotFoundException antes dele, o 404 vira 500 em silêncio.
    [Fact]
    public async Task Handle_NaoEncontrada_NaoDeveSerReembrulhadaComoErroInesperado()
    {
        SetupRepository(null);

        var act = async () => await CreateSut().Handle(new GetByIdQuery<JobViewModel>(99), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<NotFoundException>();
        thrown.Which.Message.Should().Contain("99");
    }

    [Fact]
    public async Task Handle_VagaAtiva_DeveDevolverOViewModelComOsCamposNovos()
    {
        SetupRepository(CreateJob());

        var result = await CreateSut().Handle(new GetByIdQuery<JobViewModel>(1), CancellationToken.None);

        result.Title.Should().Be("Operador(a) de Empilhadeira");
        result.City.Should().Be("Extrema");
        result.State.Should().Be(UF.MG);
        result.WorkShift.Should().Be(WorkShiftEnum.SegundoTurno);
        result.ExperienceLevel.Should().Be(ExperienceLevelEnum.AteUmAno);
        result.Area.Should().Be(JobAreaEnum.Logistica);
        result.SalaryMin.Should().Be(2300m);
        result.IsActive.Should().BeTrue();
    }
}
