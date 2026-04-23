namespace EmpregaNet.Tests.Support;

/// <summary>Dados únicos para testes de integração (evita violações de índice único no EF InMemory).</summary>
internal static class TestDataFactory
{
    public static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@test.local";

    /// <summary>13 dígitos: 55 + DDD 11 + 9 + 8 dígitos (regra BR do projeto).</summary>
    public static string UniqueBrazilianCell() => $"55119{Random.Shared.Next(10000000, 99999999):D8}";

    public static string UniqueUsername(string prefix) => $"{prefix}_{Guid.NewGuid():N}";
}
