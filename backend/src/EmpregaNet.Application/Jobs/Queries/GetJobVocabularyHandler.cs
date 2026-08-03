using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Jobs.Queries;

/// <summary>
/// Vocabulário do domínio de vagas.
/// </summary>
public sealed record GetJobVocabularyQuery : IRequest<JobVocabularyViewModel>;

public sealed class GetJobVocabularyHandler : IRequestHandler<GetJobVocabularyQuery, JobVocabularyViewModel>
{
    /// <summary>
    /// Monta a resposta uma única vez: o vocabulário é estático e reconstruí-lo por requisição
    /// seria reflexão repetida sem nenhum ganho.
    /// </summary>
    private static readonly JobVocabularyViewModel Vocabulary = Build();

    public Task<JobVocabularyViewModel> Handle(GetJobVocabularyQuery request, CancellationToken cancellationToken)
        => Task.FromResult(Vocabulary);

    private static JobVocabularyViewModel Build() => new()
    {
        JobTypes = OptionsOf<JobTypeEnum>(),
        WorkModels = OptionsOf<WorkModelEnum>(),
        WorkShifts = OptionsOf<WorkShiftEnum>(),
        ExperienceLevels = OptionsOf<ExperienceLevelEnum>(),
        Areas = OptionsOf<JobAreaEnum>(),
        States = OptionsOf<UF>(),
        Requirements = GroupsOf(JobVocabulary.RequirementGroups),
        Benefits = GroupsOf(JobVocabulary.BenefitGroups),
        MaxItemsPerJob = JobVocabulary.MaxItemsPerJob
    };

    /// <summary>Membro neutro presente em todos os enums do domínio.</summary>
    private const string NeutralMember = "NaoSelecionado";

    /// <summary>
    /// Converte o enum em opções rotuladas, preservando a ordem de declaração.
    /// </summary>
    private static IReadOnlyList<VocabularyOptionViewModel> OptionsOf<TEnum>() where TEnum : struct, Enum
        => Enum.GetValues<TEnum>()
            .Where(value => value.ToString() != NeutralMember)
            .Select(value => new VocabularyOptionViewModel(value.ToString(), ((Enum)(object)value).ToDescription()))
            .ToList();

    private static IReadOnlyList<VocabularyGroupViewModel> GroupsOf(IReadOnlyList<JobVocabularyGroup> groups)
        => groups.Select(g => new VocabularyGroupViewModel(g.Label, g.Items)).ToList();
}
