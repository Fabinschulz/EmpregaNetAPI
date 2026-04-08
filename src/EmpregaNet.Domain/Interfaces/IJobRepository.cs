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
    /// Lista vagas com filtros opcionais.
    /// </summary>
    Task<ListDataPagination<Job>> GetAllAsync(
        CancellationToken cancellationToken,
        int page,
        int size,
        string? orderBy,
        bool? isDeleted,
        bool? isActive);
}