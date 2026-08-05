using EmpregaNet.Application.JobApplications.Queries;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.JobApplications;

/// <summary>
/// Regras de paginação da listagem de candidaturas por vaga.
/// </summary>
public sealed class GetJobApplicationsByJobIdValidatorTests
{
    private readonly GetJobApplicationsByJobIdQueryValidator _validator = new();

    private static GetJobApplicationsByJobIdQuery Query(int size) =>
        new(JobId: 1, Page: 1, Size: size, Status: null, OrderBy: null);

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
}
