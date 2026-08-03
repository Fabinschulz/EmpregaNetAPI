using System.Reflection;
using EmpregaNet.Domain.Entities;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Domain.Jobs;

/// <summary>
/// Contrato de materialização: o EF Core instancia <see cref="Job"/> pelo construtor privado e
/// depois <b>atribui</b> as coleções primitivas aos campos de apoio (<c>PropertyAccessMode.Field</c>).
/// As propriedades públicas têm de refletir o que o EF escreveu.
/// </summary>
public sealed class JobMaterializationTests
{
    /// <summary>Reproduz o que o EF faz: novo <see cref="List{T}"/> atribuído ao campo.</summary>
    private static Job MaterializeWith(string[] requirements, string[] benefits)
    {
        var job = (Job)Activator.CreateInstance(typeof(Job), nonPublic: true)!;

        SetField(job, "_requirements", [.. requirements]);
        SetField(job, "_benefits", [.. benefits]);

        return job;
    }

    private static void SetField(Job job, string fieldName, List<string> value)
        => typeof(Job)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, value);

    [Fact]
    public void Requirements_DepoisDeMaterializado_DeveRefletirOCampoEscritoPeloEf()
    {
        var job = MaterializeWith(["CNH D", "NR-11 (movimentação de cargas)"], []);

        job.Requirements.Should().BeEquivalentTo("CNH D", "NR-11 (movimentação de cargas)");
    }

    [Fact]
    public void Benefits_DepoisDeMaterializado_DeveRefletirOCampoEscritoPeloEf()
    {
        var job = MaterializeWith([], ["Fretado", "Cesta Básica"]);

        job.Benefits.Should().BeEquivalentTo("Fretado", "Cesta Básica");
    }
}
