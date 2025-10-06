using EmpregaNet.Domain.Entities;

namespace EmpregaNet.Domain.Interfaces;

public interface ICompanyRepository : IBaseRepository<Company>
{
    /// <summary>
    /// Busca uma empresa pelo número de registro.
    /// </summary>
    /// <param name="registrationNumber">Número de registro da empresa.</param>
    /// <returns>Empresa encontrada ou null se não existir.</returns>
    Task<Company?> GetByRegistrationNumberAsync(string registrationNumber);

}
