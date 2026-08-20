using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Application.Users.ViewModel;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Users;

/// <summary>
/// Cobre o mapeamento da ficha do candidato. O alvo é o mapper e não o handler de propósito: toda a
/// decisão vive aqui (sentinelas de enum, idade, agregação por status), e mockar
/// <c>UserManager&lt;User&gt;</c> só acrescentaria andaime sem cobrir nada a mais.
/// </summary>
public sealed class CandidateDetailMapperTests
{
    /// <summary>02/08/2026 02:30 UTC = 01/08/2026 23:30 em Brasília.</summary>
    private static readonly DateTimeOffset Now = new(2026, 8, 2, 2, 30, 0, TimeSpan.Zero);

    private static User CandidateWith(Address? address = null, DateTimeOffset? birthDate = null)
        => new()
        {
            Id = 5,
            UserName = "QA_E2E_Tester",
            Email = "qa@example.com",
            UserType = UserTypeEnum.Candidate,
            Address = address,
            BirthDate = birthDate
        };

    private static Address AddressWith(UF state, string city = "Extrema")
        => new()
        {
            Street = "Rua A",
            ZipCode = "37640000",
            City = city,
            State = state,
            Neighborhood = "Centro",
            Number = "10"
        };

    private static CandidateDetailViewModel Map(
        User user,
        IReadOnlyDictionary<ApplicationStatusEnum, int>? applications = null)
        => user.ToCandidateDetail([], applications ?? new Dictionary<ApplicationStatusEnum, int>(), Now);

    /// <summary>
    /// <c>NaoSelecionado</c> é o membro zero de <see cref="UF"/>: sem guarda, o endereço cadastrado
    /// sem UF sairia como a string "NaoSelecionado" e o cartão exibiria "Extrema, NaoSelecionado".
    /// </summary>
    [Fact]
    public void ToCandidateDetail_UfNaoSelecionada_DeveDevolverNuloEmVezDoNomeDoSentinela()
    {
        var result = Map(CandidateWith(AddressWith(UF.NaoSelecionado)));

        result.State.Should().BeNull();
    }

    [Fact]
    public void ToCandidateDetail_UfInformada_DeveDevolverASigla()
    {
        var result = Map(CandidateWith(AddressWith(UF.MG)));

        result.State.Should().Be("MG");
        result.City.Should().Be("Extrema");
    }

    [Fact]
    public void ToCandidateDetail_SemEndereco_DeveDevolverLocalNulo()
    {
        var result = Map(CandidateWith());

        result.State.Should().BeNull();
        result.City.Should().BeNull();
    }

    [Fact]
    public void ToCandidateDetail_CidadeEmBranco_DeveDevolverNuloEmVezDeEspacos()
    {
        var result = Map(CandidateWith(AddressWith(UF.MG, city: "   ")));

        result.City.Should().BeNull();
    }

    /// <summary>
    /// Às 23:30 de Brasília a data UTC já é 02/08. Usar o dia UTC faria a idade subir na véspera.
    /// </summary>
    [Fact]
    public void ToCandidateDetail_VesperaDoAniversarioEmBrasilia_NaoDeveContarOAnoNovo()
    {
        var result = Map(CandidateWith(birthDate: new DateTimeOffset(2000, 8, 2, 0, 0, 0, TimeSpan.Zero)));

        result.Age.Should().Be(25);
    }

    [Fact]
    public void ToCandidateDetail_AniversarioJaPassadoNoAno_DeveContarOAnoCompleto()
    {
        var result = Map(CandidateWith(birthDate: new DateTimeOffset(2000, 1, 15, 0, 0, 0, TimeSpan.Zero)));

        result.Age.Should().Be(26);
    }

    [Fact]
    public void ToCandidateDetail_SemDataDeNascimento_DeveDevolverIdadeNula()
    {
        Map(CandidateWith()).Age.Should().BeNull();
    }

    [Fact]
    public void ToCandidateDetail_DataDeNascimentoNoFuturo_DeveDevolverNuloEmVezDeIdadeNegativa()
    {
        var result = Map(CandidateWith(birthDate: new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)));

        result.Age.Should().BeNull();
    }

    [Fact]
    public void ToCandidateDetail_SemCandidaturas_DeveDevolverResumoVazioENaoNulo()
    {
        var result = Map(CandidateWith());

        result.Applications.Total.Should().Be(0);
        result.Applications.ByStatus.Should().BeEmpty();
    }

    [Fact]
    public void ToCandidateDetail_ComCandidaturas_DeveSomarOTotalEOrdenarPelaMaiorContagem()
    {
        var result = Map(CandidateWith(), new Dictionary<ApplicationStatusEnum, int>
        {
            [ApplicationStatusEnum.Approved] = 1,
            [ApplicationStatusEnum.Processing] = 4,
            [ApplicationStatusEnum.Rejected] = 2
        });

        result.Applications.Total.Should().Be(7);
        result.Applications.ByStatus.Select(x => x.Status)
            .Should().Equal("Processing", "Rejected", "Approved");
    }

    /// <summary>
    /// O status viaja como <b>nome</b> do enum, não como a descrição pt-BR: quem traduz é o
    /// frontend, em <c>features/candidaturas/domain</c>.
    /// </summary>
    [Fact]
    public void ToCandidateDetail_DeveExporOStatusComoNomeDoEnum()
    {
        var result = Map(CandidateWith(), new Dictionary<ApplicationStatusEnum, int>
        {
            [ApplicationStatusEnum.Processing] = 1
        });

        result.Applications.ByStatus.Single().Status.Should().Be("Processing");
    }

    [Fact]
    public void ToCandidateDetail_DeveExporOTipoDeUtilizadorComoDescricaoPtBr()
    {
        Map(CandidateWith()).UserType.Should().Be(UserTypeEnum.Candidate.ToDescription());
    }

    /// <summary>
    /// Gênero e estado civil ficam fora da ficha por decisão registada em
    /// <see cref="CandidateDetailViewModel"/>: são atributos protegidos sem consumidor na tela.
    /// Este teste existe para que voltarem a entrar seja uma escolha, não um descuido.
    /// </summary>
    [Fact]
    public void CandidateDetailViewModel_NaoDeveExporAtributosProtegidos()
    {
        var properties = typeof(CandidateDetailViewModel).GetProperties().Select(p => p.Name);

        properties.Should().NotContain("Gender")
            .And.NotContain("CivilStatus")
            .And.NotContain("BirthDate");
    }
}
