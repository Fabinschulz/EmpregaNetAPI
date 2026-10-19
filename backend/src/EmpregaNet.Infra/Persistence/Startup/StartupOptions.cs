namespace EmpregaNet.Infra.Persistence.Startup;

/// <summary>
/// Comportamento da inicialização (seção <c>Startup</c>): espera pelo banco, migrações, seed e chaves.
/// </summary>
/// <remarks>
/// Com várias réplicas, quem faz o quê precisa ser explícito. <b>Produção:</b> rode as migrações num job à parte
/// (<c>dotnet EmpregaNet.Api.dll --migrate-only</c>) e suba as réplicas com <see cref="RunMigrations"/> = false;
/// uma réplica que encontra migração pendente se recusa a subir, em vez de atender com o schema atrasado.
/// O padrão (<c>true</c>) preserva o comportamento anterior para ambientes de uma réplica só.
/// </remarks>
public sealed class StartupOptions
{
    public const string SectionName = "Startup";

    /// <summary>Aplica as migrações pendentes no boot. Com <c>false</c>, migração pendente impede a inicialização.</summary>
    public bool RunMigrations { get; set; } = true;

    /// <summary>Garante roles e administrador iniciais (idempotente, executado sob o bloqueio de inicialização).</summary>
    public bool RunSeed { get; set; } = true;

    /// <summary>Quanto esperar o banco ficar disponível antes de desistir. Cobre a religação diária do RDS.</summary>
    public int DatabaseWaitTimeoutSeconds { get; set; } = 180;

    /// <summary>Teto do intervalo entre tentativas (o intervalo começa em 1 s e dobra).</summary>
    public int DatabaseWaitMaxDelaySeconds { get; set; } = 10;

    /// <summary>Quanto esperar outra réplica terminar a inicialização que segura o bloqueio.</summary>
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
