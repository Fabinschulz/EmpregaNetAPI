namespace EmpregaNet.Application.Jobs.ViewModel;

public sealed class JobVocabularyViewModel
{
    public required IReadOnlyList<VocabularyOptionViewModel> JobTypes { get; init; }
    public required IReadOnlyList<VocabularyOptionViewModel> WorkModels { get; init; }
    public required IReadOnlyList<VocabularyOptionViewModel> WorkShifts { get; init; }
    public required IReadOnlyList<VocabularyOptionViewModel> ExperienceLevels { get; init; }
    public required IReadOnlyList<VocabularyOptionViewModel> Areas { get; init; }
    public required IReadOnlyList<VocabularyOptionViewModel> States { get; init; }
    public required IReadOnlyList<VocabularyGroupViewModel> Requirements { get; init; }
    public required IReadOnlyList<VocabularyGroupViewModel> Benefits { get; init; }

    /// <summary>
    /// Cidades das vagas ativas, agrupadas pelo <b>código</b> da UF. Sem rótulo por extenso: o
    /// cliente é a fonte única dos rótulos de UF.
    /// </summary>
    public required IReadOnlyList<VocabularyCityGroupViewModel> Cities { get; init; }

    public int MaxItemsPerJob { get; init; }
}

public sealed record VocabularyOptionViewModel(string Value, string Label);
public sealed record VocabularyGroupViewModel(string Label, IReadOnlyList<string> Items);

/// <param name="State">Código da UF (ex.: <c>"CE"</c>), o mesmo valor aceito em <c>state[]</c> pelo feed.</param>
/// <param name="Items">Cidades distintas, em ordem alfabética.</param>
public sealed record VocabularyCityGroupViewModel(string State, IReadOnlyList<string> Items);
