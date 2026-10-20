using System.Security.Cryptography.X509Certificates;
using EmpregaNet.Infra.Persistence.Database;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmpregaNet.Infra.Extensions;

/// <summary>Seção <c>DataProtection</c>. O certificado cifra as chaves do key ring em repouso; não assina os tokens.</summary>
public sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";
    public const string DefaultApplicationName = "EmpregaNet";

    /// <summary>Sem nome fixo o framework usa o caminho do conteúdo, que muda entre imagens e invalida tokens de outra réplica.</summary>
    public string ApplicationName { get; set; } = DefaultApplicationName;

    /// <summary>PFX (PKCS#12) com chave privada, em Base64.</summary>
    public string? CertificatePfxBase64 { get; set; }

    public string? CertificatePassword { get; set; }

    /// <summary>Só para ler chaves gravadas antes de uma rotação do certificado.</summary>
    public string? PreviousCertificatePfxBase64 { get; set; }

    public string? PreviousCertificatePassword { get; set; }
}

public static class DataProtectionExtensions
{
    /// <summary>
    /// Guarda o key ring no PostgreSQL, compartilhado entre réplicas: os tokens de e-mail e de senha do Identity
    /// dependem dele. Em Production o certificado é obrigatório; nos demais ambientes a falta dele só avisa.
    /// </summary>
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
