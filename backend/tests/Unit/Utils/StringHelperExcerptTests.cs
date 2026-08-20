using EmpregaNet.Application.Utils.Helpers;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Utils;

/// <summary>Recorte de texto para exibição em cartão: <c>ToExcerpt</c> e <c>CollapseWhitespace</c>.</summary>
public sealed class StringHelperExcerptTests
{
    [Theory]
    [InlineData("  Atendimento   ao cliente.  ", "Atendimento ao cliente.")]
    [InlineData("Linha um\n\nLinha dois", "Linha um Linha dois")]
    [InlineData("Com\ttabulação", "Com tabulação")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void CollapseWhitespace_DeveNormalizarEspacos(string? entrada, string esperado)
    {
        entrada.CollapseWhitespace().Should().Be(esperado);
    }

    [Fact]
    public void ToExcerpt_TextoDentroDoLimite_DeveDevolverIntacto()
    {
        "Atendimento ao cliente.".ToExcerpt(280).Should().Be("Atendimento ao cliente.");
    }

    [Fact]
    public void ToExcerpt_TextoVazio_DeveDevolverVazioENaoReticencias()
    {
        "   ".ToExcerpt(280).Should().BeEmpty();
    }

    [Fact]
    public void ToExcerpt_AcimaDoLimite_DeveCortarNaPalavraInteira()
    {
        var texto = string.Join(' ', Enumerable.Repeat("logistica", 40));

        var excerto = texto.ToExcerpt(280);

        excerto.Should().EndWith("…");
        excerto.Length.Should().BeLessThanOrEqualTo(281);
        excerto.TrimEnd('…').Should().EndWith("logistica");
    }

    /// <summary>
    /// Texto que já vinha cortado precisa da marca mesmo cabendo no limite, senão a última palavra
    /// aparece partida sem aviso.
    /// </summary>
    [Fact]
    public void ToExcerpt_JaTruncado_DeveMarcarOCorteMesmoAbaixoDoLimite()
    {
        "Responsável pela operaç".ToExcerpt(280, alreadyTruncated: true).Should().Be("Responsável pela…");
    }

    [Fact]
    public void ToExcerpt_NaoTruncado_NaoDeveMarcarCorte()
    {
        "Responsável pela operação".ToExcerpt(280).Should().Be("Responsável pela operação");
    }

    /// <summary>
    /// Recuo até menos de metade do limite significa palavra longa demais (URL, código): cortar no
    /// limite devolve mais informação do que recuar até quase nada.
    /// </summary>
    [Fact]
    public void ToExcerpt_PalavraUnicaMaiorQueOLimite_DeveCortarNoLimite()
    {
        var excerto = new string('a', 100).ToExcerpt(20);

        excerto.Should().Be(new string('a', 19) + "…");
    }
}
