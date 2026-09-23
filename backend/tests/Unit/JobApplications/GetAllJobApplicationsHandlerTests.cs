using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Queries;
using EmpregaNet.Application.JobApplications.ViewModel;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Escopo por empresa em <c>GET /api/jobapplications</c>: Admin vê a plataforma inteira, Recruiter/Manager
/// só as candidaturas de vagas da própria empresa — o mesmo padrão de <see cref="IJobEmployerAccess"/> já
/// aplicado em <c>GetJobApplicationsByJobIdHandler</c> e <c>ChangeJobApplicationStatusCommandHandler</c>.
/// </summary>
public sealed class GetAllJobApplicationsHandlerTests
{
    private readonly Mock<IJobApplicationRepository> _repository = new();
    private readonly Mock<IJobEmployerAccess> _employerAccess = new();

    private GetAllJobApplicationsHandler CreateSut() =>
        new(_repository.Object, _employerAccess.Object, NullLogger<GetAllJobApplicationsHandler>.Instance);

    private static ListDataPagination<JobApplicationProjection> EmptyPage() =>
        new(new List<JobApplicationProjection>(), totalItems: 0, page: 1, pageSize: 100);

    [Fact]
    public async Task Handle_Admin_DeveListarSemRestricaoDeEmpresa()
    {
        _employerAccess
            .Setup(x => x.ResolveCompanyScopeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
        _repository
            .Setup(x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(), 1, 100, null, null))
            .ReturnsAsync(EmptyPage());

        var result = await CreateSut().Handle(
            new GetAllQuery<JobApplicationViewModel>(1, 100, null),
            CancellationToken.None);

        result.TotalItems.Should().Be(0);
        _repository.Verify(
            x => x.GetAllWithCandidateAsync(It.IsAny<CancellationToken>(), 1, 100, null, null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RecruiterVinculadoAEmpresa_DeveListarApenasComEscopoDaPropriaEmpresa()
    {
        const long companyId = 7;
        _employerAccess
            .Setup(x => x.ResolveCompanyScopeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)companyId);
        _repository
            .Setup(x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(), 1, 100, null, companyId))
            .ReturnsAsync(EmptyPage());

        await CreateSut().Handle(new GetAllQuery<JobApplicationViewModel>(1, 100, null), CancellationToken.None);

        _repository.Verify(
            x => x.GetAllWithCandidateAsync(It.IsAny<CancellationToken>(), 1, 100, null, companyId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UsuarioDeRecrutamentoSemVinculoAEmpresa_DevePropagarFaltaDePermissaoSemConsultarRepositorio()
    {
        _employerAccess
            .Setup(x => x.ResolveCompanyScopeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(ValidationAppException.ForBusinessRule(
                "Seu usuário ainda não está vinculado a uma empresa. Solicite ao administrador.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION));

        var act = async () => await CreateSut().Handle(
            new GetAllQuery<JobApplicationViewModel>(1, 100, null),
            CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        _repository.Verify(
            x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<long?>()),
            Times.Never);
    }
}
