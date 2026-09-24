using EmpregaNet.Application.Abstraction;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Application.JobApplications.Queries;
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

    private static GetAllJobApplicationsQuery Query() =>
        new(Page: 1, Size: 100, OrderBy: null, IsDeleted: null, Search: null, Status: null);

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
                It.IsAny<CancellationToken>(), 1, 100, null, null, null, null))
            .ReturnsAsync(EmptyPage());

        var result = await CreateSut().Handle(Query(), CancellationToken.None);

        result.TotalItems.Should().Be(0);
        _repository.Verify(
            x => x.GetAllWithCandidateAsync(It.IsAny<CancellationToken>(), 1, 100, null, null, null, null),
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
                It.IsAny<CancellationToken>(), 1, 100, null, companyId, null, null))
            .ReturnsAsync(EmptyPage());

        await CreateSut().Handle(Query(), CancellationToken.None);

        _repository.Verify(
            x => x.GetAllWithCandidateAsync(It.IsAny<CancellationToken>(), 1, 100, null, companyId, null, null),
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

        var act = async () => await CreateSut().Handle(Query(), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        _repository.Verify(
            x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<long?>(),
                It.IsAny<ApplicationStatusEnum?>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    // ---------- emp-filtros-candidaturas-recrutamento — CA-02 ----------

    [Theory]
    [InlineData("Processing", ApplicationStatusEnum.Processing)]
    [InlineData("processing", ApplicationStatusEnum.Processing)]
    [InlineData("CANCELEDBYCANDIDATE", ApplicationStatusEnum.CanceledByCandidate)]
    [InlineData(" processing ", ApplicationStatusEnum.Processing)] // espaços nas pontas + caixa diferente
    public async Task Handle_CA02_StatusValido_DeveRepassarOStatusEABuscaAoRepositorio(
        string status, ApplicationStatusEnum expected)
    {
        // Arrange
        const long companyId = 7;
        _employerAccess
            .Setup(x => x.ResolveCompanyScopeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)companyId);
        _repository
            .Setup(x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(), 1, 100, null, companyId, expected, "maria"))
            .ReturnsAsync(EmptyPage());

        // Act
        await CreateSut().Handle(Query() with { Status = status, Search = "maria" }, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.GetAllWithCandidateAsync(It.IsAny<CancellationToken>(), 1, 100, null, companyId, expected, "maria"),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_CA02_StatusVazio_NaoDeveFiltrarPorStatus(string status)
    {
        _employerAccess
            .Setup(x => x.ResolveCompanyScopeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
        _repository
            .Setup(x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(), 1, 100, null, null, null, null))
            .ReturnsAsync(EmptyPage());

        await CreateSut().Handle(Query() with { Status = status }, CancellationToken.None);

        _repository.Verify(
            x => x.GetAllWithCandidateAsync(It.IsAny<CancellationToken>(), 1, 100, null, null, null, null),
            Times.Once);
    }

    // Regressão do spec: mesmo DomainError do endpoint por vaga. "99" e "0" passam no Enum.TryParse;
    // sem a recusa explícita, filtrariam em silêncio (lista vazia / NaoSelecionado) em vez de 400.
    [Theory]
    [InlineData("99")]
    [InlineData("0")]
    [InlineData("NaoSelecionado")]
    [InlineData("naoselecionado")]
    [InlineData("Recebida")]
    [InlineData("Inexistente")]
    [InlineData("Foo")]
    [InlineData("1")]                // número de membro declarado (Approved): só o nome é aceito
    [InlineData("Approved,Pending")] // lista com vírgula: Enum.TryParse faria OU bit a bit (= Rejected)
    public async Task Handle_CA02_StatusInvalido_DeveLancarInvalidQueryFilterSemConsultarRepositorio(string status)
    {
        // Arrange
        _employerAccess
            .Setup(x => x.ResolveCompanyScopeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);

        // Act
        var act = async () => await CreateSut().Handle(Query() with { Status = status }, CancellationToken.None);

        // Assert
        var assertion = await act.Should().ThrowAsync<ValidationAppException>();
        assertion.Which.Code.Should().Be(DomainErrorEnum.INVALID_QUERY_FILTER);
        _repository.Verify(
            x => x.GetAllWithCandidateAsync(
                It.IsAny<CancellationToken>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<long?>(),
                It.IsAny<ApplicationStatusEnum?>(),
                It.IsAny<string?>()),
            Times.Never);
    }
}
