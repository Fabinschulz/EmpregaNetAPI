namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Seção <c>Startup</c>. Em produção, migre num job à parte (<c>--migrate-only</c>) e suba as réplicas com
/// <see cref="RunMigrations"/> = false; o padrão <c>true</c> mantém o comportamento de uma réplica só.
/// </summary>
public sealed class StartupOptions
{
    public const string SectionName = "Startup";

    /// <summary>Com <c>false</c>, migração pendente impede a inicialização.</summary>
    public bool RunMigrations { get; set; } = true;

    public bool RunSeed { get; set; } = true;

    public int DatabaseWaitTimeoutSeconds { get; set; } = 180;

    /// <summary>Teto do intervalo entre tentativas (começa em 1 s e dobra).</summary>
    public int DatabaseWaitMaxDelaySeconds { get; set; } = 10;

    public int LockTimeoutSeconds { get; set; } = 120;

    public void Validate()
    {
        if (DatabaseWaitTimeoutSeconds < 1)
        {
            throw new InvalidOperationException($"'{SectionName}:{nameof(DatabaseWaitTimeoutSeconds)}' deve ser maior que zero.");
        }

        if (DatabaseWaitMaxDelaySeconds < 1)
        {
            throw new InvalidOperationException($"'{SectionName}:{nameof(DatabaseWaitMaxDelaySeconds)}' deve ser maior que zero.");
        }

        if (LockTimeoutSeconds < 1)
        {
            throw new InvalidOperationException($"'{SectionName}:{nameof(LockTimeoutSeconds)}' deve ser maior que zero.");
        }
    }
}
