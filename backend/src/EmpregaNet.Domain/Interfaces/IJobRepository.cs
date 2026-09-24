using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Domain.Interfaces;

public interface IJobRepository : IBaseRepository<Job>
{
    /// <summary>
    /// Verifica se uma vaga de emprego existe pelo título e empresa.
    /// </summary>
    /// <param name="title">Título da vaga.</param>
    /// <param name="companyId">ID da empresa.</param>
    /// <returns>True se a vaga existir, caso contrário false.</returns>
    Task<bool> ExistsByTitleAndCompanyIdAsync(string title, long companyId);

    /// <summary>
    /// Obtém uma vaga de emprego pelo ID para atualização. Retorna null se a vaga não existir.
    /// </summary>
    Task<Job?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Lista vagas com filtros opcionais. <paramref name="search"/> busca por título ou descrição.
    /// Usado pela gestão de recrutamento; a descoberta pública usa <see cref="GetFeedAsync"/>.
    /// </summary>
    Task<ListDataPagination<Job>> GetAllAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy,
        bool? isDeleted,
        bool? isActive,
        string? search = null);

    /// <summary>
    /// Lista vagas com filtros opcionais. <paramref name="filter"/> busca por título, descrição, empresa, cidade, estado e país.
    /// </summary>
    Task<ListDataPagination<JobFeedProjection>> GetFeedAsync(
        JobFeedFilter filter,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cidades das vagas visíveis no feed (ativas, não excluídas, de empresa não excluída), agrupadas
    /// por UF (ordenadas pelo código da UF).
    /// Cidade vazia não entra; grafias que só diferem por espaços nas pontas colapsam numa entrada.
    /// UF sem nenhuma vaga não gera grupo.
    /// </summary>
    Task<IReadOnlyList<VocabularyCityGroup>> GetActiveCitiesByStateAsync(CancellationToken cancellationToken);
}
