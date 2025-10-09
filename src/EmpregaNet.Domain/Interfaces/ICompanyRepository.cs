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

    /// <summary>
    /// Verifica se uma empresa existe pelo CNPJ.
    /// </summary>
    /// <param name="cnpj">CNPJ da empresa.</param>
    /// <returns>True se a empresa existir, caso contrário false.</returns>
    Task<bool> ExistsByCnpjAsync(string cnpj);

}
