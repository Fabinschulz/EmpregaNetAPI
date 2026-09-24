using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Application.Utils.Helpers;
using EmpregaNet.Domain.Enums;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Application.Jobs.Queries;

/// <summary>
/// Vocabulário do domínio de vagas.
/// </summary>
public sealed record GetJobVocabularyQuery : IRequest<JobVocabularyViewModel>;

public sealed class GetJobVocabularyHandler : IRequestHandler<GetJobVocabularyQuery, JobVocabularyViewModel>
{
    /// <summary>
    /// Parte derivada de enums e constantes, montada uma única vez: é estática e reconstruí-la por
    /// requisição seria reflexão repetida sem nenhum ganho.
    /// </summary>
    private static readonly JobVocabularyViewModel StaticVocabulary = Build();

    private readonly IJobRepository _jobRepository;

    public GetJobVocabularyHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// <c>Cities</c> vem das vagas e é consultado por requisição, o que, na prática, só acontece
    /// quando o cache HTTP do endpoint (<c>PublicCatalog</c>, invalidado pela tag de <c>Job</c>) expira.
    /// </summary>
    public async Task<JobVocabularyViewModel> Handle(GetJobVocabularyQuery request, CancellationToken cancellationToken)
    {
        var groups = await _jobRepository.GetActiveCitiesByStateAsync(cancellationToken);

        return new JobVocabularyViewModel
        {
            JobTypes = StaticVocabulary.JobTypes,
            WorkModels = StaticVocabulary.WorkModels,
            WorkShifts = StaticVocabulary.WorkShifts,
            ExperienceLevels = StaticVocabulary.ExperienceLevels,
            Areas = StaticVocabulary.Areas,
            States = StaticVocabulary.States,
            Requirements = StaticVocabulary.Requirements,
            Benefits = StaticVocabulary.Benefits,
            Cities = groups
                .Select(g => new VocabularyCityGroupViewModel(g.State.ToString(), g.Items))
                .ToList(),
            MaxItemsPerJob = StaticVocabulary.MaxItemsPerJob
        };
    }

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
        Cities = [],
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
