namespace EmpregaNet.Application.Jobs.ViewModel;

/// <summary>
/// Estado do utilizador autenticado sobre as vagas consultadas.
/// </summary>
/// <remarks>
/// Devolve apenas os ids em que há candidatura, e não um mapa de todas as vagas: a ausência já é
/// a resposta "não se candidatou", e a lista costuma ser bem menor que a página consultada.
/// </remarks>
public sealed class JobFeedInteractionsViewModel
{
    public required IReadOnlyList<long> AppliedJobIds { get; init; }
}
