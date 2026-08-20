namespace EmpregaNet.Application.Abstraction;

/// <summary>
/// Invalidação do Output Cache HTTP por tags (handlers e controllers após mutações).
/// </summary>
/// <remarks>
/// Vive na Application, não no Domain: cache é preocupação de infraestrutura, tags, TTL,
/// despejo do store. O domínio não deve conhecer nada disso, e nenhuma regra de negócio depende
/// desta abstração.
/// </remarks>
public interface IOutputCacheManager
{
    /// <summary>
    /// Invalida cache de leitura de entidades (single e list) com base no nome do ViewModel e opcionalmente por ID específico.
    /// </summary>
    /// <param name="viewModelName"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    Task InvalidateEntityAsync(string viewModelName, long id = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida cache relacionada a usuários administrativos. Se um userId específico for fornecido, invalida também cache de perfil e candidatos relacionados a esse usuário.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    Task InvalidateAdminUsersAsync(long userId = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida cache de leitura de candidatos. Se um candidateId específico for fornecido, pode ser usado para invalidações mais granulares (ex: cache de perfil do candidato).
    /// </summary>
    /// <param name="candidateId"></param>
    /// <param name="cancellationToken"></param>
    Task InvalidateCandidatesAsync(long candidateId = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida cache de leitura do perfil do usuário (UserMe). O userId é necessário para construir a tag correta, já que o cache é específico por usuário.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    Task InvalidateUserMeAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida a cache afetada por uma mutação de candidatura (criar, atualizar status, excluir):
    /// a lista do próprio utilizador (JobApplicationsMine), a lista por vaga (JobApplicationsByJob)
    /// e a leitura de candidatos (Candidates) — a ficha do candidato expõe a contagem de
    /// candidaturas por status e ficaria a mostrar o quadro anterior.
    /// Nenhuma dessas caches é específica por utilizador, por isso não recebe ID.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task InvalidateJobApplicationsAsync(CancellationToken cancellationToken = default);
}
