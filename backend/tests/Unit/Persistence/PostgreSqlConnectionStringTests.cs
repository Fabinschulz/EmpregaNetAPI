using EmpregaNet.Infra.Persistence.Database;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EmpregaNet.Tests.Unit.Persistence;

public sealed class PostgreSqlConnectionStringTests
{
    private const string Raw = "Host=db;Port=5432;Database=empreganet;Username=app;Password=segredo;";

    private static NpgsqlConnectionStringBuilder Parse(string connectionString) => new(connectionString);

    [Fact]
    public void Aplica_o_pool_explicito_e_o_nome_da_aplicacao_quando_a_string_nao_os_define()
    {
        var result = Parse(PostgreSqlConnectionString.Apply(Raw, new DatabaseOptions()));

        result.MaxPoolSize.Should().Be(20, "o padrão de 100 do Npgsql estouraria um RDS pequeno com poucas réplicas");
        result.MinPoolSize.Should().Be(0);
        result.ApplicationName.Should().Be("EmpregaNet.Api");
    }

    [Fact]
    public void Valor_escrito_na_connection_string_prevalece_sobre_a_configuracao()
    {
        var raw = Raw + "Maximum Pool Size=7;Minimum Pool Size=2;Application Name=migrador;";

        var result = Parse(PostgreSqlConnectionString.Apply(raw, new DatabaseOptions { MaxPoolSize = 50 }));

        result.MaxPoolSize.Should().Be(7);
        result.MinPoolSize.Should().Be(2);
        result.ApplicationName.Should().Be("migrador");
    }

    [Fact]
    public void Preserva_as_credenciais_e_o_destino()
    {
        var result = Parse(PostgreSqlConnectionString.Apply(Raw, new DatabaseOptions()));

        result.Host.Should().Be("db");
        result.Database.Should().Be("empreganet");
        result.Username.Should().Be("app");
        result.Password.Should().Be("segredo");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 11)]
    [InlineData(10, -1)]
    public void Recusa_parametros_de_pool_incoerentes(int max, int min)
    {
        var act = () => PostgreSqlConnectionString.Apply(Raw, new DatabaseOptions { MaxPoolSize = max, MinPoolSize = min });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Resolve_le_a_connection_string_e_a_secao_Database_da_configuracao()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgreSQLConnection"] = Raw,
            ["Database:MaxPoolSize"] = "15"
        }).Build();

        Parse(PostgreSqlConnectionString.Resolve(configuration)).MaxPoolSize.Should().Be(15);
    }

    [Fact]
    public void Resolve_falha_com_mensagem_clara_sem_connection_string()
    {
        var act = () => PostgreSqlConnectionString.Resolve(new ConfigurationBuilder().Build());

        act.Should().Throw<InvalidOperationException>().WithMessage("*PostgreSQLConnection*");
    }

    [Theory]
    [InlineData(100, 2, 20, 10, true)]   // 2×20 + 10 = 50
    [InlineData(100, 2, 100, 10, false)] // o padrão do Npgsql: 2×100 + 10 = 210
    [InlineData(50, 2, 20, 10, true)]    // 2×20 + 10 = 50, no limite
    [InlineData(50, 3, 20, 10, false)]   // 3×20 + 10 = 70
    public void Calcula_se_o_total_de_conexoes_das_replicas_cabe_no_banco(
        int maxConnections, int replicas, int pool, int reserved, bool fits)
    {
        DatabaseCapacity.Fits(maxConnections, replicas, pool, reserved).Should().Be(fits);
    }

    [Fact]
    public void Calcula_o_maior_pool_seguro_contando_um_job_de_migracao()
    {
        // (100 - 10) / (2 réplicas + 1 job) = 30
        DatabaseCapacity.MaxSafePoolSize(maxConnections: 100, replicas: 2, reservedConnections: 10).Should().Be(30);
        DatabaseCapacity.MaxSafePoolSize(maxConnections: 5, replicas: 2, reservedConnections: 10).Should().Be(0);
    }
}
