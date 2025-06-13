using EmpregaNet.Domain.Components.Mapper;

namespace Mapper.Interfaces;

/// <summary>
/// Define um contrato para configuração fluente de mapeamentos entre <typeparamref name="TSource"/> e <typeparamref name="TDestination"/>.
/// Permite configurar opções personalizadas e suporte a mapeamento reverso.
/// </summary>
/// <typeparam name="TSource">Tipo de origem.</typeparam>
/// <typeparam name="TDestination">Tipo de destino.</typeparam>
public interface IMappingExpression<TSource, TDestination>
{
    /// <summary>
    /// Habilita o mapeamento reverso entre <typeparamref name="TDestination"/> e <typeparamref name="TSource"/>.
    /// </summary>
    /// <returns>Instância atual para encadeamento fluente.</returns>
    IMappingExpression<TSource, TDestination> ReverseMap();

    /// <summary>
    /// Configura uma opção personalizada de mapeamento para o membro especificado.
    /// </summary>
    /// <param name="memberName">Nome do membro de destino.</param>
    /// <param name="options">Ação que define as opções personalizadas.</param>
    /// <returns>Instância atual para encadeamento fluente.</returns>
    IMappingExpression<TSource, TDestination> ForMember(string memberName, Action<MemberOptions> options);

    /// <summary>
    /// Aplica as configurações de mapeamento realizadas.
    /// </summary>
    void Apply();
}
