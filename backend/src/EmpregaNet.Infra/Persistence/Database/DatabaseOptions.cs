namespace EmpregaNet.Infra.Persistence.Database;

/// <summary>
/// Seção <c>Database</c>. Só vale para o que a connection string não define: valor escrito nela prevalece.
/// O pool do Npgsql é por processo, então o banco recebe <c>réplicas × MaxPoolSize</c> conexões (ver <see cref="DatabaseCapacity"/>).
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Padrão do Npgsql: 100.</summary>
    public int MaxPoolSize { get; set; } = 20;

    public int MinPoolSize { get; set; }

    public string ApplicationName { get; set; } = "EmpregaNet.Api";

    /// <summary>Máximo do Auto Scaling; só alimenta o aviso de capacidade.</summary>
    public int ExpectedMaxReplicas { get; set; } = 2;

    public int ReservedConnections { get; set; } = 10;

    public void Validate()
    {
        if (MaxPoolSize < 1)
        {
            throw new InvalidOperationException($"'{SectionName}:{nameof(MaxPoolSize)}' deve ser maior que zero.");
        }

        if (MinPoolSize < 0 || MinPoolSize > MaxPoolSize)
        {
            throw new InvalidOperationException(
                $"'{SectionName}:{nameof(MinPoolSize)}' deve estar entre 0 e {nameof(MaxPoolSize)} ({MaxPoolSize}).");
        }

        if (ExpectedMaxReplicas < 1)
        {
            throw new InvalidOperationException($"'{SectionName}:{nameof(ExpectedMaxReplicas)}' deve ser maior que zero.");
        }

        if (ReservedConnections < 0)
        {
            throw new InvalidOperationException($"'{SectionName}:{nameof(ReservedConnections)}' não pode ser negativo.");
        }
    }
}

public static class DatabaseCapacity
{
    public static int Required(int replicas, int maxPoolSize, int reservedConnections) =>
        replicas * maxPoolSize + reservedConnections;

    /// <summary>Maior <c>MaxPoolSize</c> que cabe no banco, contando mais uma réplica (job de migração ou deploy).</summary>
    public static int MaxSafePoolSize(int maxConnections, int replicas, int reservedConnections) =>
        Math.Max(0, (maxConnections - reservedConnections) / (replicas + 1));

    public static bool Fits(int maxConnections, int replicas, int maxPoolSize, int reservedConnections) =>
        Required(replicas, maxPoolSize, reservedConnections) <= maxConnections;
}
