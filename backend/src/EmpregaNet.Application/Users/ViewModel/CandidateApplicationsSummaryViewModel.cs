using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Users.ViewModel;

/// <summary>Posição do candidato nos processos seletivos, agregada por status.</summary>
public sealed class CandidateApplicationsSummaryViewModel
{
    public int Total { get; init; }
    public IReadOnlyList<CandidateApplicationsByStatusViewModel> ByStatus { get; init; } = [];

    public static CandidateApplicationsSummaryViewModel Empty { get; } = new();

    public static CandidateApplicationsSummaryViewModel From(
        IReadOnlyDictionary<ApplicationStatusEnum, int> countsByStatus)
    {
        if (countsByStatus.Count == 0)
        {
            return Empty;
        }

        return new CandidateApplicationsSummaryViewModel
        {
            Total = countsByStatus.Values.Sum(),
            ByStatus = countsByStatus
                .OrderByDescending(entry => entry.Value)
                .ThenBy(entry => entry.Key.ToString(), StringComparer.Ordinal)
                .Select(CandidateApplicationsByStatusViewModel.From)
                .ToList()
        };
    }
}

public sealed class CandidateApplicationsByStatusViewModel
{
    public required string Status { get; init; }
    public int Count { get; init; }

    public static CandidateApplicationsByStatusViewModel From(KeyValuePair<ApplicationStatusEnum, int> entry)
        => new() { Status = entry.Key.ToString(), Count = entry.Value };
}
