namespace EmpregaNet.Application.Utils.Helpers;

/// <summary>
/// Converte texto em enum aceitando <b>apenas o nome de um membro declarado</b>.
/// </summary>
/// <remarks>
/// <c>Enum.TryParse</c> é permissivo demais para filtros vindos da query string: aceita números
/// (<c>"1"</c>, <c>"99"</c>) e listas com vírgula, que combina por OU bit a bit
/// (<c>"Approved,Pending"</c> vira outro membro e passa no <c>Enum.IsDefined</c>). Aqui a comparação é
/// feita direto contra os nomes declarados, o que recusa as duas formas por construção.
/// </remarks>
public static class EnumNameParser
{
    /// <summary>
    /// Procura um membro de <typeparamref name="TEnum"/> cujo nome seja igual a
    /// <paramref name="value"/> (sem distinção de caixa, espaços nas pontas ignorados).
    /// </summary>
    public static bool TryParseName<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var name = value.Trim();

        var declared = Array.Find(
            Enum.GetNames<TEnum>(),
            candidate => string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase));

        if (declared is null)
        {
            return false;
        }

        result = Enum.Parse<TEnum>(declared);
        return true;
    }
}
