using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Domain.Common;

/// <summary>
/// Cidades distintas das vagas ativas e não excluídas de uma UF, já aparadas e em ordem alfabética.
/// Lido por <c>IJobRepository.GetActiveCitiesByStateAsync</c> para o vocabulário do feed.
/// </summary>
public sealed record VocabularyCityGroup(UF State, IReadOnlyList<string> Items);
