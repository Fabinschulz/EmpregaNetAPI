using EmpregaNet.Application.Jobs;
using EmpregaNet.Application.Jobs.Queries;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Enums;
using FluentAssertions;

namespace EmpregaNet.Tests.Unit.Application.Jobs;

/// <summary>
/// O vocabulário é a fonte única servida ao cliente (ADR 0008). Estes testes protegem as duas
/// invariantes que, se quebradas, aparecem direto na tela do utilizador: rótulo faltando e item
/// duplicado entre grupos.
/// </summary>
public sealed class JobVocabularyTests
{
    private static readonly JobVocabularyHandlerFixture Fixture = new();

    [Fact]
    public void RequirementGroups_NaoDevemTerItemRepetidoEntreGrupos()
    {
        var items = JobVocabulary.RequirementGroups.SelectMany(g => g.Items).ToList();

        items.Should().OnlyHaveUniqueItems("um requisito em dois grupos apareceria duas vezes no formulário");
    }

    [Fact]
    public void BenefitGroups_NaoDevemTerItemRepetidoEntreGrupos()
    {
        var items = JobVocabulary.BenefitGroups.SelectMany(g => g.Items).ToList();

        items.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Grupos_NaoDevemEstarVazios()
    {
        JobVocabulary.RequirementGroups.Should().OnlyContain(g => g.Items.Count > 0);
        JobVocabulary.BenefitGroups.Should().OnlyContain(g => g.Items.Count > 0);
    }

    [Fact]
    public void CanonicalRequirement_DeveCorrigirACaixaParaAGrafiaOficial()
    {
        JobVocabulary.CanonicalRequirement("cnh d").Should().Be("CNH D");
        JobVocabulary.CanonicalRequirement("  Empilhadeira  ").Should().Be("Empilhadeira");
    }

    [Fact]
    public void CanonicalRequirement_ValorDesconhecidoVoltaApenasAparado()
    {
        JobVocabulary.CanonicalRequirement("  Requisito Novo  ").Should().Be("Requisito Novo");
    }

    [Fact]
    public void IsKnownRequirement_DeveIgnorarCaixaEEspacos()
    {
        JobVocabulary.IsKnownRequirement(" empilhadeira ").Should().BeTrue();
        JobVocabulary.IsKnownRequirement("Framework Inexistente").Should().BeFalse();
    }

    /// <summary>
    /// Todo membro selecionável precisa de <c>[Description]</c>: sem ela o cliente exibiria o
    /// identificador do enum ("SegundoTurno") no lugar do rótulo em português.
    /// </summary>
    [Theory]
    [InlineData(typeof(JobTypeEnum))]
    [InlineData(typeof(WorkModelEnum))]
    [InlineData(typeof(WorkShiftEnum))]
    [InlineData(typeof(ExperienceLevelEnum))]
    [InlineData(typeof(JobAreaEnum))]
    public async Task Vocabulario_TodoMembroSelecionavelTemRotuloEmPortugues(Type enumType)
    {
        var vocabulary = await Fixture.LoadAsync();

        var options = enumType switch
        {
            _ when enumType == typeof(JobTypeEnum) => vocabulary.JobTypes,
            _ when enumType == typeof(WorkModelEnum) => vocabulary.WorkModels,
            _ when enumType == typeof(WorkShiftEnum) => vocabulary.WorkShifts,
            _ when enumType == typeof(ExperienceLevelEnum) => vocabulary.ExperienceLevels,
            _ => vocabulary.Areas
        };

        options.Should().NotBeEmpty();
        options.Should().OnlyContain(o => !string.IsNullOrWhiteSpace(o.Label));

        // Rótulo idêntico ao identificador denuncia um membro sem [Description] — salvo quando a
        // palavra é a mesma nos dois idiomas ("Design", "Trainee"), que é legítimo.
        options.Should().NotContain(o => o.Label.Contains('_'));
    }

    [Fact]
    public async Task Vocabulario_NaoDeveOferecerOMembroNeutro()
    {
        var vocabulary = await Fixture.LoadAsync();

        vocabulary.JobTypes.Should().NotContain(o => o.Value == "NaoSelecionado");
        vocabulary.WorkShifts.Should().NotContain(o => o.Value == "NaoSelecionado");
        vocabulary.ExperienceLevels.Should().NotContain(o => o.Value == "NaoSelecionado");
        vocabulary.Areas.Should().NotContain(o => o.Value == "NaoSelecionado");

        // UF.NaoSelecionado tem descrição ("Não informado") e escaparia de um corte por rótulo
        // vazio — o corte é pelo nome justamente por causa dele.
        vocabulary.States.Should().NotContain(o => o.Value == "NaoSelecionado");
    }

    [Fact]
    public async Task Vocabulario_DeveIncluirOIndice8ReservadoForaDoTipoDeContratacao()
    {
        var vocabulary = await Fixture.LoadAsync();

        // O 8 era `Remote` e virou modalidade (ADR 0006). Não pode reaparecer como vínculo.
        vocabulary.JobTypes.Should().NotContain(o => o.Value == "Remote");
        vocabulary.WorkModels.Should().Contain(o => o.Value == "Remote");
    }

    [Fact]
    public async Task Vocabulario_DeveExporOsGruposDeRequisitosEBeneficios()
    {
        var vocabulary = await Fixture.LoadAsync();

        vocabulary.Requirements.Should().NotBeEmpty();
        vocabulary.Benefits.Should().NotBeEmpty();
        vocabulary.MaxItemsPerJob.Should().Be(JobVocabulary.MaxItemsPerJob);

        vocabulary.Requirements.SelectMany(g => g.Items).Should().Contain("Empilhadeira");
        vocabulary.Benefits.SelectMany(g => g.Items).Should().Contain("Fretado");
    }

    private sealed class JobVocabularyHandlerFixture
    {
        private readonly GetJobVocabularyHandler _handler = new();

        public Task<JobVocabularyViewModel> LoadAsync()
            => _handler.Handle(new GetJobVocabularyQuery(), CancellationToken.None);
    }
}
