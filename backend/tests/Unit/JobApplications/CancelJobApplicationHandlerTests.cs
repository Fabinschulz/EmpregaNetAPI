using EmpregaNet.Application.Auth;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Commands;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Posse e estado no cancelamento pelo candidato. O ponto sensível é a <b>uniformidade do 404</b>:
/// candidatura inexistente e candidatura de outra pessoa têm de sair pela mesma porta, senão o
/// endpoint responde a "este id existe?" a quem só tem de saber dos seus próprios registos.
/// </summary>
public sealed class CancelJobApplicationHandlerTests
{
    private const long CandidateId = 10;
    private const long OtherCandidateId = 20;
    private const long ApplicationId = 500;

    private readonly Mock<IJobApplicationRepository> _repository = new();
    private readonly Mock<IHttpCurrentUser> _currentUser = new();

    private CancelJobApplicationHandler CreateSut() =>
        new(_repository.Object, _currentUser.Object, NullLogger<CancelJobApplicationHandler>.Instance);

    private void GivenAuthenticatedUser(long userId, params string[] roles)
    {
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _currentUser.Setup(x => x.GetContextUser()).Returns(new UserLoggedViewModel
        {
            AccessToken = "token",
            ExpiresIn = 3600,
            UserToken = new UserToken
            {
                Id = userId,
                Username = "candidato",
                Email = "candidato@test.local",
                Roles = roles.Length == 0 ? ["Candidate"] : [.. roles],
                Claims = []
            }
        });
    }

    private static JobApplication CreateApplication(long userId, ApplicationStatusEnum status)
    {
        var application = new JobApplication(jobId: 1, userId: userId);

        if (status != ApplicationStatusEnum.Pending)
        {
            application.ChangeStatus(status);
        }

        return application;
    }

    private void GivenStoredApplication(JobApplication? application)
    {
        _repository
            .Setup(x => x.GetByIdAsync(ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        if (application is null)
        {
            return;
        }

        _repository
            .Setup(x => x.UpdateAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _repository
            .Setup(x => x.GetProjectionByIdAsync(ApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new JobApplicationProjection(
                ApplicationId,
                application.JobId,
                new JobApplicationCandidate(application.UserId, "Candidato", "candidato@test.local", false),
                application.Status,
                application.AppliedAt,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null,
                false));
    }

    [Fact]
    public async Task Handle_CandidaturaPropriaEmAnalise_DeveCancelarEPersistir()
    {
        GivenAuthenticatedUser(CandidateId);
        var application = CreateApplication(CandidateId, ApplicationStatusEnum.Processing);
        GivenStoredApplication(application);

        var result = await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        result.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        application.Status.Should().Be(ApplicationStatusEnum.CanceledByCandidate);
        _repository.Verify(x => x.UpdateAsync(application, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CandidaturaInexistente_DeveLancarNotFound()
    {
        GivenAuthenticatedUser(CandidateId);
        GivenStoredApplication(null);

        var act = async () => await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Candidatura não encontrada.");
    }

    // A recusa da candidatura alheia tem de ser indistinguível da inexistente — mesma excepção,
    // mesma mensagem. Qualquer diferença aqui vira um oráculo de ids no endpoint.
    [Fact]
    public async Task Handle_CandidaturaDeOutroUtilizador_DeveLancarOMesmoNotFoundDaInexistente()
    {
        GivenAuthenticatedUser(CandidateId);
        GivenStoredApplication(CreateApplication(OtherCandidateId, ApplicationStatusEnum.Processing));

        var act = async () => await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Candidatura não encontrada.");
        _repository.Verify(x => x.UpdateAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CandidaturaExcluida_DeveLancarNotFound()
    {
        GivenAuthenticatedUser(CandidateId);
        var application = CreateApplication(CandidateId, ApplicationStatusEnum.Processing);
        application.IsDeleted = true;
        GivenStoredApplication(application);

        var act = async () => await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(ApplicationStatusEnum.Approved)]
    [InlineData(ApplicationStatusEnum.Rejected)]
    [InlineData(ApplicationStatusEnum.Finished)]
    [InlineData(ApplicationStatusEnum.Canceled)]
    public async Task Handle_EstadoQueNaoPermiteCancelar_DeveLancarValidacaoDeStatus(ApplicationStatusEnum status)
    {
        GivenAuthenticatedUser(CandidateId);
        GivenStoredApplication(CreateApplication(CandidateId, status));

        var act = async () => await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_STATUS);
        _repository.Verify(x => x.UpdateAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Cancelar como empresa é a mudança de status já existente; deixar o recrutamento passar por
    // aqui gravaria o ato da empresa com a autoria do candidato.
    [Theory]
    [InlineData(RecruitmentRoleNames.Admin)]
    [InlineData(RecruitmentRoleNames.Recruiter)]
    [InlineData(RecruitmentRoleNames.Manager)]
    public async Task Handle_PerfilDeRecrutamento_DeveRecusarAntesDeConsultarACandidatura(string role)
    {
        GivenAuthenticatedUser(CandidateId, role);

        var act = async () => await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        _repository.Verify(x => x.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SemUtilizadorNoContexto_DeveLancarFaltaDePermissao()
    {
        _currentUser.Setup(x => x.GetContextUser()).Returns((UserLoggedViewModel?)null);

        var act = async () => await CreateSut().Handle(new CancelJobApplicationCommand(ApplicationId), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
    }
}
