using System.Text.Json;
using EmpregaNet.Api.Middleware;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmpregaNet.Tests.Unit.Api;

/// <summary>
/// Objetivo: garantir o contrato do corpo de erro - que a falha chega ligada ao campo que a
/// originou, que o caminho é o que o cliente enviou, e que a mensagem de topo é a frase útil.
/// </summary>
public sealed class GlobalExceptionHandlerTests
{
    private static readonly JsonSerializerOptions ReadOptions = new(JsonSerializerDefaults.Web);

    private static async Task<DomainError> HandleAsync(Exception exception)
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var context = new DefaultHttpContext();
        context.Items["Correlation-ID"] = "correlation-de-teste";
        using var body = new MemoryStream();
        context.Response.Body = body;

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();

        body.Position = 0;
        var domainError = await JsonSerializer.DeserializeAsync<DomainError>(body, ReadOptions);
        return domainError!;
    }

    [Fact]
    public async Task RegraDeNegocio_DeveSairSemCampoEComAPropriaFraseNaMensagem()
    {
        // Given - o caso do botão "Candidatar-se": não há formulário, logo não há campo a destacar.
        var exception = ValidationAppException.ForBusinessRule(
            "Apenas candidatos podem se candidatar para vagas.",
            DomainErrorEnum.INVALID_ACTION_FOR_RECORD);

        // When
        var error = await HandleAsync(exception);

        // Then
        error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        error.Code.Should().Be(DomainErrorEnum.INVALID_ACTION_FOR_RECORD);
        error.Message.Should().Be("Apenas candidatos podem se candidatar para vagas.");
        error.Errors.Should().ContainSingle()
            .Which.Field.Should().BeNull();
    }

    [Fact]
    public async Task FalhaUnicaDeCampo_DeveUsarAPropriaFraseEmVezDoTituloGenerico()
    {
        // Given
        var exception = new ValidationAppException("Cnpj", "CNPJ inválido.", DomainErrorEnum.INVALID_PARAMS);

        // When
        var error = await HandleAsync(exception);

        // Then
        error.Message.Should().Be("CNPJ inválido.");
        error.Errors.Should().ContainSingle()
            .Which.Field.Should().Be("cnpj");
    }

    [Fact]
    public async Task VariasFalhas_DeveUsarTituloGenericoEPreservarCadaCampo()
    {
        // Given
        var exception = new ValidationAppException(
        [
            new ValidationFailure("entity.CompanyName", "O nome da empresa é obrigatório."),
            new ValidationFailure("entity.Address.ZipCode", "O CEP é obrigatório.")
        ]);

        // When
        var error = await HandleAsync(exception);

        // Then
        error.Message.Should().Be("Corrija os campos destacados.");
        error.Errors.Select(e => e.Field).Should().Equal("companyName", "address.zipCode");
    }

    [Fact]
    public async Task CaminhoDoCampo_DeveDescartarOEnvelopeEPreservarOIndiceDaColecao()
    {
        // Given - `entity` existe só no CreateCommand<T>; o cliente nunca o enviou.
        var exception = new ValidationAppException(
        [
            new ValidationFailure("entity.Requirements[2].Name", "Requisito inválido.")
        ]);

        // When
        var error = await HandleAsync(exception);

        // Then
        error.Errors.Should().ContainSingle()
            .Which.Field.Should().Be("requirements[2].name");
    }

    [Fact]
    public async Task ExcecaoInesperada_NaoDeveVazarDetalheInternoParaOCliente()
    {
        // Given
        var exception = new Exception("Connection string 'Default' inválida.");

        // When
        var error = await HandleAsync(exception);

        // Then
        error.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        error.Message.Should().Be("Erro interno no servidor.");
        error.Errors.Should().BeEmpty();
        error.StackTrace.Should().BeNull();
    }
}
