namespace EmpregaNet.Infra.Persistence.Database;

/// <summary>
/// Parâmetros de conexão com o PostgreSQL (seção <c>Database</c>). Valem só para o que a connection string não
/// definir explicitamente: um valor escrito nela (por exemplo <c>Maximum Pool Size=10</c>) sempre prevalece.
/// </summary>
/// <remarks>
/// O pool do Npgsql é <b>por processo</b>. O total de conexões que o banco recebe é
/// <c>réplicas × MaxPoolSize</c>, mais o job de migração e o acesso administrativo, e precisa caber em
/// <c>max_connections</c> do RDS menos as conexões reservadas. O padrão do Npgsql (100 por processo) estouraria
/// um <c>db.t4g.micro</c> com poucas réplicas. Veja <see cref="DatabaseCapacity"/>.
/// </remarks>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Conexões máximas por réplica. Padrão de Npgsql: 100.</summary>
    public int MaxPoolSize { get; set; } = 20;

    /// <summary>Conexões mantidas abertas mesmo ociosas. Zero evita segurar conexão em réplica parada.</summary>
    public int MinPoolSize { get; set; }

    /// <summary>Aparece em <c>pg_stat_activity</c> e permite distinguir a API do job de migração.</summary>
    public string ApplicationName { get; set; } = "EmpregaNet.Api";

    /// <summary>Número máximo de réplicas previsto (o <c>max</c> do Auto Scaling). Só alimenta o aviso de capacidade.</summary>
    public int ExpectedMaxReplicas { get; set; } = 2;

    /// <summary>Conexões do banco reservadas para administração e para o job de migração.</summary>
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

/// <summary>Conta simples de capacidade de conexões do banco.</summary>
public static class DatabaseCapacity
{
    /// <summary>Conexões que o banco pode receber no pior caso: todas as réplicas com o pool cheio.</summary>
    public static int Required(int replicas, int maxPoolSize, int reservedConnections) =>
        replicas * maxPoolSize + reservedConnections;

    /// <summary>Maior <c>MaxPoolSize</c> por réplica que ainda cabe no banco, contando um job de migração.</summary>
    public static int MaxSafePoolSize(int maxConnections, int replicas, int reservedConnections) =>
        Math.Max(0, (maxConnections - reservedConnections) / (replicas + 1));

    public static bool Fits(int maxConnections, int replicas, int maxPoolSize, int reservedConnections) =>
        Required(replicas, maxPoolSize, reservedConnections) <= maxConnections;
}
