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
    public int MaxItemsPerJob { get; init; }
}

public sealed record VocabularyOptionViewModel(string Value, string Label);
public sealed record VocabularyGroupViewModel(string Label, IReadOnlyList<string> Items);
