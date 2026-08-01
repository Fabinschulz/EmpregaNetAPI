using EmpregaNet.Application.Utils.CustomValidation;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Validation;

/// <summary>
/// Validação de CPF e CNPJ por dígito verificador.
/// </summary>
/// <remarks>
/// A regra de comando conferia só o formato, então <c>11111111111111</c> era aceito como empresa.
/// Sequências repetidas merecem teste próprio: <c>111.111.111-11</c> <b>passa</b> no algoritmo de
/// CPF, e sem a guarda explícita entraria como documento legítimo.
/// </remarks>
public sealed class BrazilianDocumentTests
{
    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    public void IsValidCnpj_DocumentoValido_DeveAceitarComOuSemMascara(string cnpj)
    {
        BrazilianDocument.IsValidCnpj(cnpj).Should().BeTrue();
    }

    [Theory]
    [InlineData("11222333000182", "dígito verificador errado")]
    [InlineData("1122233300018", "13 dígitos")]
    [InlineData(null, "nulo")]
    public void IsValidCnpj_DocumentoInvalido_DeveRecusar(string? cnpj, string motivo)
    {
        BrazilianDocument.IsValidCnpj(cnpj).Should().BeFalse(motivo);
    }

    [Theory]
    [InlineData("00000000000000")]
    [InlineData("11111111111111")]
    public void IsValidCnpj_DigitosRepetidos_DeveRecusar(string cnpj)
    {
        BrazilianDocument.IsValidCnpj(cnpj).Should().BeFalse();
    }

    // ---------- CNPJ alfanumérico (IN RFB nº 2.229/2024) ----------

    [Theory]
    [InlineData("12ABC34501DE35")]
    [InlineData("12.ABC.345/01DE-35")]
    [InlineData("12abc34501de35")]
    public void IsValidCnpj_AlfanumericoValido_DeveAceitar(string cnpj)
    {
        // Exemplo de referência divulgado pela Receita Federal para o formato alfanumérico.
        BrazilianDocument.IsValidCnpj(cnpj).Should().BeTrue();
    }

    [Theory]
    [InlineData("12ABC34501DE36", "dígito verificador errado")]
    [InlineData("12ABC34501DEX5", "verificador não pode ser letra")]
    public void IsValidCnpj_AlfanumericoInvalido_DeveRecusar(string cnpj, string motivo)
    {
        BrazilianDocument.IsValidCnpj(cnpj).Should().BeFalse(motivo);
    }

    [Theory]
    [InlineData("12.ABC.345/01DE-35", "12ABC34501DE35")]
    [InlineData("12abc34501de35", "12ABC34501DE35")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    public void NormalizeCnpj_DeveRemoverMascaraEManterLetrasEmMaiusculas(string entrada, string esperado)
    {
        // Regressão central: um "só números" gravaria "1234501" e a empresa deixaria de ser
        // encontrável pelo documento informado.
        BrazilianDocument.NormalizeCnpj(entrada).Should().Be(esperado);
    }

    [Theory]
    [InlineData("12ABC34501DE35", "12.ABC.345/01DE-35")]
    [InlineData("11222333000181", "11.222.333/0001-81")]
    public void FormatCnpj_DeveAplicarMascaraPreservandoLetras(string cnpj, string esperado)
    {
        BrazilianDocument.FormatCnpj(cnpj).Should().Be(esperado);
    }

    // ---------- CPF (permanece numérico) ----------

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void IsValidCpf_DocumentoValido_DeveAceitar(string cpf)
    {
        BrazilianDocument.IsValidCpf(cpf).Should().BeTrue();
    }

    [Fact]
    public void IsValidCpf_ComLetra_DeveRecusar()
    {
        BrazilianDocument.IsValidCpf("5299822472A").Should().BeFalse("apenas o CNPJ passou a ser alfanumérico");
    }

    [Fact]
    public void IsValidCpf_DigitosRepetidos_DeveRecusarAindaQuePassemNaAritmetica()
    {
        // 111.111.111-11 produz DV 1 e 1 pelo algoritmo: é o caso que só a guarda explícita pega.
        BrazilianDocument.IsValidCpf("11111111111").Should().BeFalse();
    }
}
