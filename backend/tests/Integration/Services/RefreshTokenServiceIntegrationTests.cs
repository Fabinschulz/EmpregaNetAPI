using EmpregaNet.Application.Abstraction;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Services;

/// <summary>
/// Objetivo: validar a rotação de refresh tokens e a detecção de reuso —
/// garantindo que a segurança da conta do usuário seja mantida.
/// </summary>
/// <remarks>
/// Cenários:
/// 1. Rotação de token válido: deve emitir um novo token e revogar o antigo.
/// 2. Rotação de token desconhecido: deve devolver null, indicando que o token não é válido.
/// 3. Reuso de token revogado: deve revogar todos os tokens ativos do usuário, prevenindo ataques de reuso.
/// 4. Rotação sem reuso: deve garantir que outros tokens do usuário não sejam afetados, mantendo a independência das sessões.
/// </remarks>
[Collection("Integration")]
public sealed class RefreshTokenServiceIntegrationTests
{
    private readonly InMemoryIdentityFixture _fx;

    public RefreshTokenServiceIntegrationTests(InMemoryIdentityFixture fx)
    {
        _fx = fx;
    }

    private IRefreshTokenService Sut(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

    [Fact]
    public async Task Rotate_TokenValido_DeveEmitirNovoERevogarOAntigo()
    {
        var userId = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fx.Services, TestDataFactory.UniqueEmail("rotate"), "rotate");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = Sut(scope);

        var original = await sut.IssueAsync(userId);
        var rotated = await sut.RotateAsync(original);

        rotated.Should().NotBeNull("o token original é válido");
        rotated!.Value.User.Id.Should().Be(userId);
        rotated.Value.NewRefreshToken.Should().NotBe(original, "a rotação emite um token novo");
    }

    [Fact]
    public async Task Rotate_TokenDesconhecido_DeveDevolverNull()
    {
        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = Sut(scope);

        (await sut.RotateAsync("token-que-nunca-existiu")).Should().BeNull();
        (await sut.RotateAsync("")).Should().BeNull();
    }

    [Fact]
    public async Task Rotate_ReusoDeTokenRevogado_DeveRevogarTodaAFamiliaDoUsuario()
    {
        var userId = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fx.Services, TestDataFactory.UniqueEmail("reuse"), "reuse");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = Sut(scope);

        // Vítima loga (RT1) e renova normalmente (RT1 → RT2).
        var rt1 = await sut.IssueAsync(userId);
        var rotated = await sut.RotateAsync(rt1);
        rotated.Should().NotBeNull();
        var rt2 = rotated!.Value.NewRefreshToken;

        // Atacante (ou vítima, indistinguível) reapresenta o RT1 já revogado.
        (await sut.RotateAsync(rt1)).Should().BeNull("token revogado nunca volta a valer");

        // Consequência da detecção de reuso: o RT2 (ainda "válido" até então) também caiu.
        (await sut.RotateAsync(rt2)).Should().BeNull("o reuso derruba a família inteira do usuário");
    }

    [Fact]
    public async Task Rotate_SemReuso_NaoDeveAfetarOutrosTokensDoUsuario()
    {
        var userId = await AuthIntegrationTestHelper.RegisterConfirmedUserAsync(
            _fx.Services, TestDataFactory.UniqueEmail("multi"), "multi");

        await using var scope = _fx.Services.CreateAsyncScope();
        var sut = Sut(scope);

        // Duas sessões independentes (ex.: desktop e celular).
        var desktop = await sut.IssueAsync(userId);
        var mobile = await sut.IssueAsync(userId);

        // Rotação legítima no desktop não pode derrubar o celular.
        (await sut.RotateAsync(desktop)).Should().NotBeNull();
        (await sut.RotateAsync(mobile)).Should().NotBeNull("rotação legítima é isolada por sessão");
    }
}
