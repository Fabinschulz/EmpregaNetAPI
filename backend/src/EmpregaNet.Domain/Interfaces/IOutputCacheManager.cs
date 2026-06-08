namespace EmpregaNet.Domain.Interfaces;

/// <summary>
/// Invalidação do Output Cache HTTP por tags (handlers e controllers após mutações).
/// </summary>
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
    /// Invalida cache relacionada a aplicações de emprego, tanto a lista de aplicações do usuário autenticado (JobApplicationsMine)
    /// quanto a lista de aplicações por vaga (JobApplicationsByJob).
    /// Essa invalidação é importante após mutações que possam afetar o estado das aplicações (ex: criar, atualizar ou excluir uma aplicação).
    /// Como essas caches não são específicas por usuário, não é necessário um ID para a invalidação. 
    /// O método pode ser chamado sempre que houver uma mudança relevante nas aplicações de emprego para garantir que os dados em cache sejam atualizados corretamente.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task InvalidateJobApplicationsAsync(CancellationToken cancellationToken = default);
}
