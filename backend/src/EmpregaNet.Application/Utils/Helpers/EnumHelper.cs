using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reflection;

namespace EmpregaNet.Application.Utils.Helpers;

/// <summary>
/// Rótulos de apresentação dos enums do domínio, declarados via <see cref="DescriptionAttribute"/>.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Mapa nome para rótulo por tipo de enum.
    /// </summary>
    /// <remarks>
    /// A leitura dos atributos é feita uma vez por tipo. Sem o cache, cada item de lista mapeado
    /// para ViewModel pagaria reflexão por campo, no caminho quente do feed e das listagens.
    /// </remarks>
    private static readonly ConcurrentDictionary<Type, FrozenDictionary<string, string>> LabelsByType = new();

    /// <summary>
    /// Obtém a descrição do valor definida pelo <see cref="DescriptionAttribute"/>.
    /// </summary>
    /// <param name="value">Valor do enum.</param>
    /// <returns>
    /// A descrição do atributo; na ausência dele, o nome do membro. String vazia quando o valor é
    /// nulo ou não corresponde a nenhum membro declarado.
    /// </returns>
    /// <example>
    /// <code>
    /// public enum Status
    /// {
    ///     [Description("Ativo")] Active,
    ///     [Description("Inativo")] Inactive
    /// }
    ///
    /// Status.Active.ToDescription(); // "Ativo"
    /// </code>
    /// </example>
    public static string ToDescription(this Enum value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var labels = LabelsByType.GetOrAdd(value.GetType(), BuildLabels);

        return labels.GetValueOrDefault(value.ToString(), string.Empty);
    }

    private static FrozenDictionary<string, string> BuildLabels(Type enumType)
        => enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .ToFrozenDictionary(
                field => field.Name,
                field => field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name,
                StringComparer.Ordinal);
}
