using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EmpregaNet.Infra.Persistence.Database;

/// <summary>
/// Única fonte da connection string: aplica os parâmetros de <see cref="DatabaseOptions"/> que a string não definiu,
/// para DbContext, teste de disponibilidade e bloqueio de inicialização abrirem conexões com as mesmas regras.
/// </summary>
public static class PostgreSqlConnectionString
{
    public const string Name = "PostgreSQLConnection";

    public static string Resolve(IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString(Name)
                  ?? throw new InvalidOperationException("PostgreSQLConnection não encontrada no arquivo de configuração.");

        var options = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        return Apply(raw, options);
    }

    public static string Apply(string rawConnectionString, DatabaseOptions options)
    {
        options.Validate();

        // O builder do Npgsql responde "sim" a ContainsKey para qualquer chave conhecida, escrita ou não; o
        // builder genérico só enxerga o que realmente está na string.
        var written = new DbConnectionStringBuilder { ConnectionString = rawConnectionString };
        var builder = new NpgsqlConnectionStringBuilder(rawConnectionString);

        if (!HasAny(written, "Maximum Pool Size", "Max Pool Size", "MaxPoolSize"))
        {
            builder.MaxPoolSize = options.MaxPoolSize;
        }

        if (!HasAny(written, "Minimum Pool Size", "Min Pool Size", "MinPoolSize"))
        {
            builder.MinPoolSize = options.MinPoolSize;
        }

        if (!HasAny(written, "Application Name", "ApplicationName") && !string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            builder.ApplicationName = options.ApplicationName;
        }

        if (builder.MinPoolSize > builder.MaxPoolSize)
        {
            throw new InvalidOperationException(
                $"Minimum Pool Size ({builder.MinPoolSize}) não pode passar de Maximum Pool Size ({builder.MaxPoolSize}).");
        }

        return builder.ConnectionString;
    }

    private static bool HasAny(DbConnectionStringBuilder builder, params string[] keys) =>
        keys.Any(builder.ContainsKey);
}
