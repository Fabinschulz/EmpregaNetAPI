using System.Security.Cryptography.X509Certificates;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmpregaNet.Infra.Extensions;

/// <summary>
/// Configuração da seção <c>DataProtection</c>.
/// </summary>
/// <remarks>
/// O certificado protege as <b>chaves</b> do key ring em repouso; ele não assina nem cifra os tokens. Os
/// valores vêm de segredo (ECS <c>secrets</c>, Parameter Store ou Secrets Manager), nunca do repositório.
/// </remarks>
public sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";
    public const string DefaultApplicationName = "EmpregaNet";

    /// <summary>
    /// Isola o key ring desta aplicação. Sem um nome fixo o ASP.NET Core usa o caminho do conteúdo,
    /// que muda entre imagens e quebraria a validação de tokens emitidos por outra réplica.
    /// </summary>
    public string ApplicationName { get; set; } = DefaultApplicationName;

    /// <summary>PFX (PKCS#12) com chave privada, em Base64. Cifra as chaves antes de gravá-las no banco.</summary>
    public string? CertificatePfxBase64 { get; set; }

    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Certificado anterior, usado só para <b>ler</b> chaves gravadas antes de uma rotação do certificado.
    /// Remova-o depois que o key ring tiver sido regravado com o certificado novo.
    /// </summary>
    public string? PreviousCertificatePfxBase64 { get; set; }

    public string? PreviousCertificatePassword { get; set; }
}

public static class DataProtectionExtensions
{
    /// <summary>
    /// Persiste o key ring do Data Protection no PostgreSQL, compartilhado por todas as réplicas.
    /// </summary>
    /// <remarks>
    /// Tokens de confirmação de e-mail e de redefinição de senha do Identity são protegidos por este key
    /// ring. Com o padrão do framework cada contêiner gera as próprias chaves em disco efêmero: o link
    /// emitido por uma réplica seria inválido em outra e depois de qualquer reinício.
    /// <para>Em <b>Production</b> o certificado é obrigatório, para que o banco nunca guarde as chaves em
    /// texto claro. Nos demais ambientes (inclusive Staging, que o pipeline atual usa) a ausência dele só
    /// gera um aviso, para não interromper os ambientes existentes.</para>
    /// <para>Nenhuma chave é gerada ou apagada a cada partida: o framework só cria uma chave nova quando
    /// não há nenhuma ativa. A primeira chave é criada pelo coordenador de inicialização, sob bloqueio.</para>
    /// </remarks>
    public static IServiceCollection AddEmpregaNetDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var settings = configuration.GetSection(DataProtectionSettings.SectionName).Get<DataProtectionSettings>()
                       ?? new DataProtectionSettings();

        var applicationName = string.IsNullOrWhiteSpace(settings.ApplicationName)
            ? DataProtectionSettings.DefaultApplicationName
            : settings.ApplicationName.Trim();

        var primary = LoadCertificate(
            settings.CertificatePfxBase64, settings.CertificatePassword, nameof(settings.CertificatePfxBase64));

        if (primary is null && environment.IsProduction())
        {
            throw new InvalidOperationException(
                $"Produção: '{DataProtectionSettings.SectionName}:{nameof(settings.CertificatePfxBase64)}' é obrigatório. " +
                "As chaves do Data Protection ficam no banco e precisam ser cifradas em repouso.");
        }

        var builder = services.AddDataProtection()
            .SetApplicationName(applicationName)
            .PersistKeysToDbContext<PostgreSqlContext>();

        if (primary is null)
        {
            Console.WriteLine(
                "Aviso: DataProtection sem certificado; as chaves ficam em texto claro no banco. " +
                "Obrigatório em Production.");
            return services;
        }

        builder.ProtectKeysWithCertificate(primary);

        var previous = LoadCertificate(
            settings.PreviousCertificatePfxBase64,
            settings.PreviousCertificatePassword,
            nameof(settings.PreviousCertificatePfxBase64));

        if (previous is not null)
        {
            builder.UnprotectKeysWithAnyCertificate(previous);
        }

        return services;
    }

    private static X509Certificate2? LoadCertificate(string? pfxBase64, string? password, string settingName)
    {
        if (string.IsNullOrWhiteSpace(pfxBase64))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(pfxBase64.Trim());
            var certificate = X509CertificateLoader.LoadPkcs12(
                bytes, password, X509KeyStorageFlags.EphemeralKeySet);

            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("o PFX não contém chave privada");
            }

            return certificate;
        }
        catch (Exception ex) when (ex is FormatException or System.Security.Cryptography.CryptographicException or InvalidOperationException)
        {
            // A mensagem não inclui o valor: o PFX e a senha são segredos.
            throw new InvalidOperationException(
                $"'{DataProtectionSettings.SectionName}:{settingName}' inválido ({ex.GetType().Name}). " +
                "Esperado: PFX com chave privada, em Base64, e a senha correspondente.", ex);
        }
    }
}
