using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

/// <summary>
/// Limites do feed. A rota é anónima, então cada teto aqui é também uma barreira de abuso barata.
/// </summary>
public sealed class GetJobsFeedValidatorTests
{
    private readonly GetJobsFeedValidator _validator = new();

    [Fact]
    public void Validate_QueryPadrao_DeveSerValida()
    {
        _validator.Validate(new GetJobsFeedQuery()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TamanhoDePaginaAcimaDoTeto_DeveFalhar()
    {
        var result = _validator.Validate(new GetJobsFeedQuery(Size: GetJobsFeedValidator.MaxPageSize + 1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetJobsFeedQuery.Size));
    }

    [Fact]
    public void Validate_TamanhoDePaginaNoTeto_DeveSerValido()
    {
        _validator.Validate(new GetJobsFeedQuery(Size: GetJobsFeedValidator.MaxPageSize))
                  .IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PaginaZero_DeveFalhar()
    {
        _validator.Validate(new GetJobsFeedQuery(Page: 0)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_BuscaAcimaDe120Caracteres_DeveFalhar()
    {
        var result = _validator.Validate(new GetJobsFeedQuery(Search: new string('a', 121)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("120"));
    }

    [Fact]
    public void Validate_MaisValoresQueOTetoNumFiltro_DeveFalhar()
    {
        var requirements = Enumerable.Range(0, GetJobsFeedValidator.MaxFilterValues + 1)
                                     .Select(i => $"requisito-{i}")
                                     .ToArray();

        _validator.Validate(new GetJobsFeedQuery(Requirements: requirements))
                  .IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TetoSalarialMenorQueOPiso_DeveFalhar()
    {
        var result = _validator.Validate(new GetJobsFeedQuery(SalaryMin: 9000m, SalaryMax: 4000m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetJobsFeedQuery.SalaryMax));
    }

    [Fact]
    public void Validate_PisoSalarialNegativo_DeveFalhar()
    {
        _validator.Validate(new GetJobsFeedQuery(SalaryMin: -1m)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ApenasTetoSalarial_DeveSerValido()
    {
        _validator.Validate(new GetJobsFeedQuery(SalaryMax: 4000m)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FiltrosCombinadosDentroDosLimites_DeveSerValido()
    {
        var query = new GetJobsFeedQuery(
            Search: "desenvolvedor",
            States: [UF.MG],
            WorkModels: [WorkModelEnum.Remote],
            Requirements: ["Empilhadeira", "WMS"],
            SalaryMin: 4000m,
            SalaryMax: 9000m,
            PublishedWithin: JobPublishedWindowEnum.Last7Days,
            Sort: JobFeedSortEnum.Relevance);

        _validator.Validate(query).IsValid.Should().BeTrue();
    }
}
