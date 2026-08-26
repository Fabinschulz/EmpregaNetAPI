namespace EmpregaNet.Tests.Support;

/// <summary>Dados únicos para testes de integração (evita violações de índice único no EF InMemory).</summary>
internal static class TestDataFactory
{
    public static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@test.local";

    /// <summary>13 dígitos: 55 + DDD 11 + 9 + 8 dígitos (regra BR do projeto).</summary>
    public static string UniqueBrazilianCell() => $"55119{Random.Shared.Next(10000000, 99999999):D8}";

    /// <summary>
    /// CPF válido e único, com os dígitos verificadores realmente calculados.
    /// </summary>
    /// <remarks>
    /// Sortear 11 dígitos ao acaso não serve: o validador confere o módulo 11 e o cadastro seria
    /// recusado. Os 9 primeiros dígitos vêm do aleatório e os 2 últimos são derivados deles.
    /// </remarks>
    public static string UniqueCpf()
    {
        var digits = new int[11];
        for (var i = 0; i < 9; i++)
        {
            digits[i] = Random.Shared.Next(0, 10);
        }

        // Evita as sequências de dígito repetido, que o validador rejeita por convenção.
        if (digits.Take(9).Distinct().Count() == 1)
        {
            digits[0] = (digits[0] + 1) % 10;
        }

        digits[9] = CheckDigit(digits, length: 9);
        digits[10] = CheckDigit(digits, length: 10);

        return string.Concat(digits);
    }

    /// <summary>Aplica a máscara <c>000.000.000-00</c> a um CPF de 11 dígitos.</summary>
    public static string MaskCpf(string cpf) =>
        $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";

    private static int CheckDigit(int[] digits, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += digits[i] * (length + 1 - i);
        }

        var remainder = sum * 10 % 11;
        return remainder == 10 ? 0 : remainder;
    }

    public static string UniqueUsername(string prefix) => $"{prefix}_{Guid.NewGuid():N}";
}
