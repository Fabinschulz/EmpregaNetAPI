namespace EmpregaNet.Application.Utils.CustomValidation;

/// <summary>
/// Validação e normalização de CPF e CNPJ, incluindo o CNPJ alfanumérico.
/// </summary>
/// <remarks>
/// <para><b>CNPJ alfanumérico (IN RFB nº 2.229/2024).</b> A partir de julho de 2026 as 12 primeiras
/// posições aceitam letras maiúsculas além de dígitos; os 2 dígitos verificadores continuam
/// numéricos. O cálculo é o mesmo módulo 11 de sempre, com uma única mudança: o valor de cada
/// posição passa a ser <c>código ASCII − 48</c>, o que dá 0–9 para os dígitos e 17–42 para A–Z.
/// Como <c>'0'</c> vale 0, <c>'1'</c> vale 1 e assim por diante, o algoritmo é compatível para trás:
/// todo CNPJ numérico existente continua válido sob a mesma conta, sem exigir migração de dados.</para>
///
/// <para><b>Dígitos repetidos.</b> A rejeição explícita não é redundante. <c>00000000000000</c>
/// <b>satisfaz</b> a aritmética do CNPJ (soma zero produz dígitos zero) e <c>111.111.111-11</c>
/// satisfaz a do CPF. Sem essa guarda, ambos entrariam como documentos legítimos.</para>
/// </remarks>
public static class BrazilianDocument
{
    /// <summary>Quantidade de posições de um CNPJ, com ou sem letras.</summary>
    public const int CnpjLength = 14;

    /// <summary>Posições alfanuméricas; as duas restantes são os dígitos verificadores.</summary>
    private const int CnpjBaseLength = 12;

    /// <summary>Deslocamento ASCII: <c>'0'</c> vale 0 e <c>'A'</c> vale 17.</summary>
    private const int AsciiValueOffset = '0';

    private static readonly int[] CnpjFirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CnpjSecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    /// <summary>
    /// Forma canônica do CNPJ: sem máscara e em maiúsculas.
    /// </summary>
    /// <remarks>
    /// É o que deve ser gravado e usado como chave de busca. Continuar aplicando um "só números"
    /// aqui apagaria as letras, dois CNPJs alfanuméricos diferentes virariam a mesma chave e
    /// colidiriam no índice único de <c>RegistrationNumber</c>.
    /// </remarks>
    public static string NormalizeCnpj(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Create(
            value.Count(char.IsAsciiLetterOrDigit),
            value,
            static (destination, source) =>
            {
                var index = 0;

                foreach (var character in source)
                {
                    if (char.IsAsciiLetterOrDigit(character))
                    {
                        destination[index++] = char.ToUpperInvariant(character);
                    }
                }
            });
    }

    /// <summary>Valida um CNPJ numérico ou alfanumérico, com ou sem máscara.</summary>
    public static bool IsValidCnpj(string? value)
    {
        var cnpj = NormalizeCnpj(value);

        if (cnpj.Length != CnpjLength || AllCharactersEqual(cnpj))
        {
            return false;
        }

        for (var i = 0; i < CnpjBaseLength; i++)
        {
            if (!char.IsAsciiLetterUpper(cnpj[i]) && !char.IsAsciiDigit(cnpj[i]))
            {
                return false;
            }
        }

        if (!char.IsAsciiDigit(cnpj[12]) || !char.IsAsciiDigit(cnpj[13]))
        {
            return false;
        }

        return cnpj[12] - '0' == CalculateCnpjCheckDigit(cnpj, CnpjFirstWeights)
            && cnpj[13] - '0' == CalculateCnpjCheckDigit(cnpj, CnpjSecondWeights);
    }

    /// <summary>Indica se o CNPJ informado usa o formato alfanumérico.</summary>
    public static bool IsAlphanumericCnpj(string? value) =>
        NormalizeCnpj(value).Any(char.IsAsciiLetter);

    /// <summary>Aplica a máscara <c>00.000.000/0000-00</c> preservando letras.</summary>
    public static string FormatCnpj(string? value)
    {
        var cnpj = NormalizeCnpj(value);

        if (cnpj.Length != CnpjLength)
        {
            return value?.Trim() ?? string.Empty;
        }

        return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";
    }

    /// <summary>Valida um CPF, aceitando máscara. CPF permanece exclusivamente numérico.</summary>
    public static bool IsValidCpf(string? value)
    {
        var digits = OnlyDigits(value);

        if (digits.Length != 11 || AllCharactersEqual(digits))
        {
            return false;
        }

        return digits[9] - '0' == CalculateCpfCheckDigit(digits, length: 9)
            && digits[10] - '0' == CalculateCpfCheckDigit(digits, length: 10);
    }

    /// <summary>Valida CPF ou CNPJ, decidindo pelo comprimento da forma canônica.</summary>
    public static bool IsValidCpfOrCnpj(string? value)
    {
        var normalized = NormalizeCnpj(value);

        return normalized.Length switch
        {
            11 => IsValidCpf(normalized),
            CnpjLength => IsValidCnpj(normalized),
            _ => false
        };
    }

    private static int CalculateCnpjCheckDigit(string cnpj, int[] weights)
    {
        var sum = 0;

        for (var i = 0; i < weights.Length; i++)
        {
            sum += (cnpj[i] - AsciiValueOffset) * weights[i];
        }

        var remainder = sum % 11;

        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static int CalculateCpfCheckDigit(string digits, int length)
    {
        var sum = 0;

        for (var i = 0; i < length; i++)
        {
            sum += (digits[i] - '0') * (length + 1 - i);
        }

        var remainder = sum * 10 % 11;

        return remainder == 10 ? 0 : remainder;
    }

    private static bool AllCharactersEqual(string value) => value.All(c => c == value[0]);

    private static string OnlyDigits(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsAsciiDigit).ToArray());
}
