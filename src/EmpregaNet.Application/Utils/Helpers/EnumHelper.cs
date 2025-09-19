using System.ComponentModel;
using System.Reflection;

namespace EmpregaNet.Application.Utils.Helpers;

/// <summary>
/// Classe auxiliar para operações com tipos Enum.
/// Fornece métodos de extensão para facilitar a manipulação e exibição de enums, especialmente para obter descrições amigáveis.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Obtém a descrição definida pelo atributo <see cref="DescriptionAttribute"/> de um valor Enum.
    /// Caso o atributo não esteja presente, retorna o nome do valor Enum.
    /// Se ocorrer algum erro ou o valor for nulo, retorna uma string vazia.
    /// </summary>
    /// <param name="value">Valor do Enum para o qual se deseja obter a descrição.</param>
    /// <returns>
    /// A descrição definida pelo <see cref="DescriptionAttribute"/> ou, se não houver, o nome do valor Enum.
    /// Retorna string vazia em caso de erro ou valor nulo.
    /// </returns>
    /// <example>
    /// <code>
    /// public enum Status
    /// {
    ///     [Description("Ativo")]
    ///     Active,
    ///     [Description("Inativo")]
    ///     Inactive
    /// }
    /// 
    /// var status = Status.Active;
    /// string descricao = status.ToDescription(); // "Ativo"
    /// </code>
    /// </example>
    public static string ToDescription(this Enum value)
    {
        if (value == null)
        {
            return "";
        }

        try
        {
            // Obtém o FieldInfo correspondente ao valor do Enum
            FieldInfo fi = value.GetType()!.GetField(value.ToString())!;
            // Busca o atributo DescriptionAttribute, se existir
            DescriptionAttribute[]? attributes = fi!.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                // Retorna a descrição definida no atributo
                return attributes.First().Description;
            }

            // Se não houver atributo, retorna o nome do Enum
            return value.ToString();
        }
        catch
        {
            // Em caso de erro, retorna string vazia
            return string.Empty;
        }
    }

    public static int GetEnumFromDescription(string description, Type enumType)
    {
        foreach (var field in enumType.GetFields())
        {
            DescriptionAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute ?? null!;
            if (attribute == null)
                continue;
            if (description != null && (attribute.Description?.ToLower()?.Contains(description.ToLower())) == true)
            {
                return (int)(field.GetValue(null) ?? 0);
            }
        }
        return 0;
    }

    public static List<EnumValue> GetValues(Type enumType)
    {
        List<EnumValue> values = new List<EnumValue>();
        foreach (var itemType in Enum.GetValues(enumType))
        {
            string description = string.Empty;

            try
            {
                var itemName = itemType?.ToString();
                var fieldInfo = itemName != null ? enumType.GetField(itemName) : null;
                var attributes = fieldInfo?.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);
                description = attributes != null && attributes.Length > 0
                    ? ((DescriptionAttribute)attributes[0]).Description
                    : itemType?.ToString() ?? string.Empty;
            }
            catch
            {
                description = itemType?.ToString() ?? string.Empty;
            }
            values.Add(new EnumValue()
            {
                Name = description,
                Value = itemType?.ToString() ?? string.Empty,
                ValueAsInt = itemType != null ? (int)itemType : 0
            });
        }
        return values;
    }

    public static string GetEnumDescription(Enum value)
    {
        return GetEnumDescription(value, "");
    }

    public static string GetEnumDescription(Enum value, string enumPrefix)
    {
        if (value is null)
        {
            return string.Empty;
        }
        try
        {
            FieldInfo? fi = value.GetType().GetField(string.Format("{1}{0}", value.ToString(), enumPrefix));

            if (fi == null)
            {
                return value.ToString();
            }

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    public class EnumValue
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int ValueAsInt { get; set; }
    }

}
