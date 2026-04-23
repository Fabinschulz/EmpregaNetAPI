namespace EmpregaNet.Infra.Persistence.Database;

/// <summary>
/// Credenciais e opções do seed inicial (roles e usuário administrador).
/// Em produção, defina <c>Seed__AdminPassword</c> (e preferencialmente altere o e-mail) via variáveis de ambiente ou secrets.
/// </summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = "admin@empreganet.com";

    public string AdminUserName { get; set; } = "admin";

    /// <summary>
    /// Senha inicial do admin. Deve atender às regras do Identity (mín. 8 caracteres, dígito e caractere não alfanumérico).
    /// </summary>
    public string AdminPassword { get; set; } = "Mudar@123";
}
