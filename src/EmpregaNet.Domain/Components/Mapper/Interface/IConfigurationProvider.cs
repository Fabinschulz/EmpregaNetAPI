namespace EmpregaNet.Domain.Components.Mapper.Interfaces;

/// <summary>
/// Define um contrato para o provedor de configuração de mapeamentos.
/// Responsável por fornecer informações sobre mapeamentos registrados.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Obtém o delegate de mapeamento entre os tipos especificados.
    /// </summary>
    /// <param name="source">Tipo de origem.</param>
    /// <param name="destination">Tipo de destino.</param>
    /// <returns>Delegate de mapeamento, ou null se não encontrado.</returns>
    Delegate? GetMapping(Type source, Type destination);

    /// <summary>
    /// Verifica se existe um mapeamento registrado entre os tipos especificados.
    /// </summary>
    /// <param name="source">Tipo de origem.</param>
    /// <param name="destination">Tipo de destino.</param>
    /// <returns>True se existir; caso contrário, false.</returns>
    bool HasMapping(Type source, Type destination);

    /// <summary>
    /// Obtém todas as combinações de tipos registrados para mapeamento.
    /// </summary>
    /// <returns>Enumerable de tuplas (Source, Destination).</returns>
    IEnumerable<(Type Source, Type Destination)> GetAllMappings();
}
