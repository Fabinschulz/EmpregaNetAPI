using EmpregaNet.Application.JobApplications.Queries;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Regras de paginação da listagem de candidaturas por vaga.
/// </summary>
public sealed class GetJobApplicationsByJobIdValidatorTests
{
    private readonly GetJobApplicationsByJobIdQueryValidator _validator = new();

    private static GetJobApplicationsByJobIdQuery Query(int size, string? search = null) =>
        new(JobId: 1, Page: 1, Size: size, Status: null, OrderBy: null, Search: search);

    /// <summary>
    /// O seletor de itens por página oferece 10, 20, 50 e 100: os quatro têm de passar. A regra já
    /// esteve invertida (mínimo de 100), e a tela ficava inutilizável em três das quatro opções.
    /// </summary>
    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public void Size_DasOpcoesDaUi_DeveSerAceito(int size)
    {
        var result = _validator.Validate(Query(size));

        result.IsValid.Should().BeTrue(because: string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(501)]
    public void Size_ForaDoIntervaloSuportado_DeveSerRejeitado(int size)
    {
        var result = _validator.Validate(Query(size));

        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// O validator não valida <c>Status</c>: a fonte única é o <c>ApplicationStatusParser</c> no handler,
    /// que responde <c>INVALID_QUERY_FILTER</c>. Uma regra aqui recusaria antes, com outro código de erro.
    /// </summary>
    [Theory]
    [InlineData("Foo")]
    [InlineData("1")]
    [InlineData("Approved,Pending")]
    [InlineData("NaoSelecionado")]
    public void Status_Invalido_NaoDeveSerRecusadoPeloValidator(string status)
    {
        var result = _validator.Validate(Query(size: 20) with { Status = status });

        result.IsValid.Should().BeTrue(because: string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void GetAllJobApplicationsValidator_StatusInvalido_NaoDeveSerRecusado()
    {
        var validator = new GetAllJobApplicationsValidator();

        var result = validator.Validate(new GetAllJobApplicationsQuery(
            Page: 1, Size: 20, OrderBy: null, IsDeleted: null, Search: null, Status: "Approved,Pending"));

        result.IsValid.Should().BeTrue(because: string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    /// <summary>Busca com o mesmo teto de 120 caracteres do feed de vagas.</summary>
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("maria@empresa.com", true)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    public void Search_DeveRespeitarOTetoDeCaracteres(object? search, bool expectedValid)
    {
        var term = search is int length ? new string('a', length) : (string?)search;

        var result = _validator.Validate(Query(size: 20, search: term));

        result.IsValid.Should().Be(expectedValid);
    }
}
