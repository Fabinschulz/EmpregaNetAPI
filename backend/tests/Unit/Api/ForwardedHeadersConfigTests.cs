using EmpregaNet.Api.Configuration;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Api;

public sealed class ForwardedHeadersConfigTests
{
    private static ForwardedHeadersConfig.ForwardedHeadersSettings Networks(params string[] networks) =>
        new() { KnownNetworks = networks };

    [Theory]
    [InlineData("10.0.0.0/16")]
    [InlineData("10.0.32.0/20")]
    [InlineData("172.31.0.0/24")]
    [InlineData("10.0.1.5/32")]
    [InlineData("2001:db8:abcd::/56")]
    public void Aceita_redes_do_tamanho_de_uma_subnet_ou_vpc(string cidr)
    {
        var options = ForwardedHeadersConfig.BuildOptions(Networks(cidr));

        options.KnownIPNetworks.Should().ContainSingle();
        options.ForwardLimit.Should().Be(1);
    }

    [Theory]
    [InlineData("0.0.0.0/0")]
    [InlineData("10.0.0.0/8")]
    [InlineData("172.16.0.0/12")]
    [InlineData("::/0")]
    [InlineData("2001:db8::/32")]
    public void Recusa_redes_amplas_demais_que_confiariam_em_enderecos_fora_da_infraestrutura(string cidr)
    {
        var act = () => ForwardedHeadersConfig.BuildOptions(Networks(cidr));

        act.Should().Throw<InvalidOperationException>().WithMessage("*ampla demais*");
    }

    [Theory]
    [InlineData("não-é-cidr")]
    [InlineData("10.0.0.0/99")]
    [InlineData("")]
    public void Recusa_cidr_invalido_dizendo_qual_chave_esta_errada(string cidr)
    {
        var act = () => ForwardedHeadersConfig.BuildOptions(Networks(cidr));

        act.Should().Throw<InvalidOperationException>().WithMessage("*KnownNetworks:0*");
    }

    [Fact]
    public void Recusa_ip_de_proxy_invalido()
    {
        var act = () => ForwardedHeadersConfig.BuildOptions(new() { KnownProxies = ["300.1.1.1"] });

        act.Should().Throw<InvalidOperationException>().WithMessage("*KnownProxies:0*");
    }

    [Fact]
    public void Nao_confia_em_loopback_a_menos_que_esteja_configurado()
    {
        var options = ForwardedHeadersConfig.BuildOptions(Networks("10.0.0.0/16"));

        options.KnownProxies.Should().BeEmpty();
        options.KnownIPNetworks.Should().ContainSingle().Which.ToString().Should().Be("10.0.0.0/16");
    }
}
