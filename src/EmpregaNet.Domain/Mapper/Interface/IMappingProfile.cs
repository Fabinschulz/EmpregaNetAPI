using Mapper.Implementations;

namespace EmpregaNet.Domain.Mapper.Interface;

/// <summary>
/// Define um contrato para configuração centralizada de perfis de mapeamento.
/// Permite registrar múltiplos mapeamentos diretamente no <see cref="MappingRegistry"/>.
/// </summary>
public interface IMappingProfile
{
    /// <summary>
    /// Configura os mapeamentos utilizando o <see cref="MappingRegistry"/>.
    /// </summary>
    /// <param name="registry">Instância do <see cref="MappingRegistry"/> para registro dos mapeamentos.</param>
    void Configure(MappingRegistry registry);
}
