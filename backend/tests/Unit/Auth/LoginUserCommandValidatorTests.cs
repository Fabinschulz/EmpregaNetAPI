using EmpregaNet.Application.Auth.Commands;
using FluentAssertions;
using FluentValidation;

namespace EmpregaNet.Tests.Unit.Auth;

/// <summary>
/// Objetivo: garantir que o identificador de login aceita <b>apenas</b> CPF ou e-mail — e que nome
/// de usuário não é, em nenhuma variação, uma credencial válida.
/// </summary>
public sealed class LoginUserCommandValidatorTests
{
    private const string ValidCpf = "52998224725";
    private const string ValidPassword = "Abcd@123";

    private readonly IValidator<LoginUserCommand> _sut = new LoginUserCommandValidator();

    [Theory]
    [InlineData("candidato@test.local", "e-mail")]
    [InlineData("52998224725", "CPF sem máscara")]
    [InlineData("529.982.247-25", "CPF com máscara")]
    [InlineData("  529.982.247-25  ", "CPF com espaços em volta")]
    public void Validate_IdentificadorAceito_DevePassar(string identifier, string forma)
    {
        var result = _sut.Validate(new LoginUserCommand(identifier, ValidPassword));

        result.IsValid.Should().BeTrue(forma);
    }

    [Theory]
    [InlineData("", "vazio")]
    [InlineData("   ", "só espaços")]
    public void Validate_IdentificadorVazio_DeveFalhar(string identifier, string motivo)
    {
        var result = _sut.Validate(new LoginUserCommand(identifier, ValidPassword));

        result.IsValid.Should().BeFalse(motivo);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Identifier));
    }

    [Theory]
    [InlineData("52998224726", "dígito verificador errado")]
    [InlineData("1234567890", "10 dígitos")]
    [InlineData("11111111111", "dígitos repetidos")]
    public void Validate_CpfInvalido_DeveFalhar(string identifier, string motivo)
    {
        var result = _sut.Validate(new LoginUserCommand(identifier, ValidPassword));

        result.IsValid.Should().BeFalse(motivo);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Identifier));
    }

    [Theory]
    [InlineData("@test.local", "sem parte local")]
    [InlineData("candidato@", "sem domínio")]
    [InlineData("a@b@c.local", "dois arrobas")]
    public void Validate_EmailInvalido_DeveFalhar(string identifier, string motivo)
    {
        var result = _sut.Validate(new LoginUserCommand(identifier, ValidPassword));

        result.IsValid.Should().BeFalse(motivo);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Identifier));
    }

    [Theory]
    [InlineData("candidato1", "nome de usuário simples")]
    [InlineData("candidato_1", "nome de usuário com separador")]
    [InlineData("admin", "nome de usuário administrativo")]
    public void Validate_NomeDeUsuario_DeveSerRecusadoComoIdentificador(string username, string forma)
    {
        // Nome de usuário não é credencial: não tem forma de e-mail nem de CPF, logo é recusado
        // antes de qualquer consulta — não há fallback que o aceite mais adiante.
        var result = _sut.Validate(new LoginUserCommand(username, ValidPassword));

        result.IsValid.Should().BeFalse(forma);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Identifier));
    }

    [Fact]
    public void Validate_SenhaVazia_DeveFalhar()
    {
        var result = _sut.Validate(new LoginUserCommand(ValidCpf, string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginUserCommand.Password));
    }
}
