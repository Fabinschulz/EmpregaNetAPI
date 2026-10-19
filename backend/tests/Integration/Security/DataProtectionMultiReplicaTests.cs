using EmpregaNet.Domain.Entities;
using EmpregaNet.Infra.Persistence.Database;
using EmpregaNet.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Tests.Integration.Security;

/// <summary>
/// Prova, ponta a ponta, que o key ring do Data Protection é compartilhado entre réplicas e sobrevive a
/// reinícios: um token de Identity emitido por uma réplica tem de ser aceito por outra.
/// </summary>
public sealed class DataProtectionMultiReplicaTests
{
    private const string Password = "Abcd@123";

    private static async Task<long> CreateUserAsync(ReplicaHost replica)
    {
        using var scope = replica.Scope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User
        {
            UserName = TestDataFactory.UniqueUsername("dp"),
            Email = TestDataFactory.UniqueEmail("dp"),
            Cpf = TestDataFactory.UniqueCpf()
        };

        var created = await users.CreateAsync(user, Password);
        created.Succeeded.Should().BeTrue(string.Join("; ", created.Errors.Select(e => e.Description)));
        return user.Id;
    }

    private static async Task<string> IssueEmailTokenAsync(ReplicaHost replica, long userId)
    {
        using var scope = replica.Scope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = (await users.FindByIdAsync(userId.ToString()))!;
        return await users.GenerateEmailConfirmationTokenAsync(user);
    }

    private static async Task<IdentityResult> ConfirmEmailAsync(ReplicaHost replica, long userId, string token)
    {
        using var scope = replica.Scope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = (await users.FindByIdAsync(userId.ToString()))!;
        return await users.ConfirmEmailAsync(user, token);
    }

    [Fact]
    public async Task Token_de_confirmacao_emitido_por_uma_replica_e_aceito_por_outra()
    {
        var database = ReplicaDatabase.NewInMemory();
        using var replicaA = ReplicaHost.Create(database);
        using var replicaB = ReplicaHost.Create(database);

        var userId = await CreateUserAsync(replicaA);
        var token = await IssueEmailTokenAsync(replicaA, userId);

        var result = await ConfirmEmailAsync(replicaB, userId, token);

        result.Succeeded.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    [Fact]
    public async Task Token_de_redefinicao_de_senha_emitido_por_uma_replica_e_aceito_por_outra()
    {
        var database = ReplicaDatabase.NewInMemory();
        using var replicaA = ReplicaHost.Create(database);
        using var replicaB = ReplicaHost.Create(database);

        var userId = await CreateUserAsync(replicaA);

        string token;
        using (var scope = replicaA.Scope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            token = await users.GeneratePasswordResetTokenAsync((await users.FindByIdAsync(userId.ToString()))!);
        }

        using var scopeB = replicaB.Scope();
        var usersB = scopeB.ServiceProvider.GetRequiredService<UserManager<User>>();
        var result = await usersB.ResetPasswordAsync((await usersB.FindByIdAsync(userId.ToString()))!, token, "Nova@1234");

        result.Succeeded.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    [Fact]
    public async Task Token_continua_valido_depois_que_a_aplicacao_reinicia()
    {
        var database = ReplicaDatabase.NewInMemory();
        long userId;
        string token;

        using (var antes = ReplicaHost.Create(database))
        {
            userId = await CreateUserAsync(antes);
            token = await IssueEmailTokenAsync(antes, userId);
        }

        using var depois = ReplicaHost.Create(database);
        var result = await ConfirmEmailAsync(depois, userId, token);

        result.Succeeded.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    [Fact]
    public async Task Controle_sem_chaves_compartilhadas_o_token_de_outra_replica_e_recusado()
    {
        // Reproduz o comportamento anterior à correção (chave efêmera por contêiner) e mostra que o teste
        // acima enxerga o defeito: sem persistência, a outra réplica não consegue validar o token.
        var database = ReplicaDatabase.NewInMemory();
        using var replicaA = ReplicaHost.Create(database, sharedDataProtection: false);
        using var replicaB = ReplicaHost.Create(database, sharedDataProtection: false);

        var userId = await CreateUserAsync(replicaA);
        var token = await IssueEmailTokenAsync(replicaA, userId);

        var result = await ConfirmEmailAsync(replicaB, userId, token);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Uma_segunda_replica_reaproveita_o_key_ring_e_nao_gera_chave_nova()
    {
        var database = ReplicaDatabase.NewInMemory();
        using var replicaA = ReplicaHost.Create(database);

        var userId = await CreateUserAsync(replicaA);
        await IssueEmailTokenAsync(replicaA, userId);

        using var replicaB = ReplicaHost.Create(database);
        await IssueEmailTokenAsync(replicaB, userId);

        using var scope = replicaB.Scope();
        var keys = await scope.ServiceProvider.GetRequiredService<PostgreSqlContext>()
            .DataProtectionKeys.AsNoTracking().ToListAsync();

        keys.Should().HaveCount(1, "o key ring é um só, compartilhado; reiniciar ou escalar não gera chave nova");
    }

    [Fact]
    public async Task Com_certificado_as_chaves_ficam_cifradas_no_banco()
    {
        var database = ReplicaDatabase.NewInMemory();
        var settings = new Dictionary<string, string?>
        {
            ["DataProtection:CertificatePfxBase64"] = ReplicaHost.CreateCertificatePfxBase64("senha-teste"),
            ["DataProtection:CertificatePassword"] = "senha-teste"
        };

        using var replicaA = ReplicaHost.Create(database, settings);
        using var replicaB = ReplicaHost.Create(database, settings);

        var userId = await CreateUserAsync(replicaA);
        var token = await IssueEmailTokenAsync(replicaA, userId);

        using (var scope = replicaA.Scope())
        {
            var xml = (await scope.ServiceProvider.GetRequiredService<PostgreSqlContext>()
                .DataProtectionKeys.AsNoTracking().SingleAsync()).Xml!;

            xml.Should().Contain("EncryptedData").And.Contain("EncryptedKey");
            xml.Should().NotContain("masterKey", "o material da chave não pode ficar em texto claro");
        }

        // Cifradas em repouso e ainda assim legíveis por outra réplica que tenha o mesmo certificado.
        var result = await ConfirmEmailAsync(replicaB, userId, token);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Sem_certificado_fora_de_producao_as_chaves_ficam_em_texto_claro_e_a_replica_sobe()
    {
        var database = ReplicaDatabase.NewInMemory();
        using var replica = ReplicaHost.Create(database, environment: "Staging");

        var userId = await CreateUserAsync(replica);
        await IssueEmailTokenAsync(replica, userId);

        using var scope = replica.Scope();
        var xml = (await scope.ServiceProvider.GetRequiredService<PostgreSqlContext>()
            .DataProtectionKeys.AsNoTracking().SingleAsync()).Xml!;

        xml.Should().Contain("masterKey");
    }

    [Fact]
    public void Em_producao_sem_certificado_a_aplicacao_se_recusa_a_subir()
    {
        var act = () => ReplicaHost.Create(ReplicaDatabase.NewInMemory(), environment: "Production");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CertificatePfxBase64*obrigatório*");
    }

    [Fact]
    public void Certificado_invalido_e_recusado_sem_vazar_o_valor()
    {
        var settings = new Dictionary<string, string?>
        {
            ["DataProtection:CertificatePfxBase64"] = "isto-nao-e-base64-valido!!",
            ["DataProtection:CertificatePassword"] = "segredo-que-nao-pode-aparecer"
        };

        var act = () => ReplicaHost.Create(ReplicaDatabase.NewInMemory(), settings);

        var thrown = act.Should().Throw<InvalidOperationException>().Which;
        thrown.Message.Should().NotContain("segredo-que-nao-pode-aparecer");
        thrown.Message.Should().NotContain("isto-nao-e-base64-valido");
    }

    [Fact]
    public async Task Rotacao_o_certificado_anterior_ainda_le_as_chaves_gravadas_com_ele()
    {
        var database = ReplicaDatabase.NewInMemory();
        var antigo = ReplicaHost.CreateCertificatePfxBase64("antigo");
        var novo = ReplicaHost.CreateCertificatePfxBase64("novo", "CN=empreganet-dp-novo");

        long userId;
        string token;
        using (var antes = ReplicaHost.Create(database, new Dictionary<string, string?>
        {
            ["DataProtection:CertificatePfxBase64"] = antigo,
            ["DataProtection:CertificatePassword"] = "antigo"
        }))
        {
            userId = await CreateUserAsync(antes);
            token = await IssueEmailTokenAsync(antes, userId);
        }

        using var depois = ReplicaHost.Create(database, new Dictionary<string, string?>
        {
            ["DataProtection:CertificatePfxBase64"] = novo,
            ["DataProtection:CertificatePassword"] = "novo",
            ["DataProtection:PreviousCertificatePfxBase64"] = antigo,
            ["DataProtection:PreviousCertificatePassword"] = "antigo"
        });

        var result = await ConfirmEmailAsync(depois, userId, token);
        result.Succeeded.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
