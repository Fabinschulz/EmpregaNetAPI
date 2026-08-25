namespace EmpregaNet.Api.Configuration;

/// <summary>
/// Nomes das políticas registradas em <see cref="OutputCacheConfig"/>.
/// Endpoints opt-in via <c>[OutputCache(PolicyName = ...)]</c>; a política base desabilita cache por padrão.
/// </summary>
public static class OutputCachePolicies
{
    /// <summary>GET público (ex.: catálogo de vagas).</summary>
    public const string PublicCatalog = "PublicCatalog";

    /// <summary>GET autenticado do <see cref="Controllers.Core.MainController{TCreateCommand,TUpdateCommand,TViewModel}"/> (tags de entidade dinâmicas).</summary>
    public const string EntityRead = "EntityRead";

    /// <summary>GET autenticado genérico; combine com <c>Tags</c> no atributo para invalidação por domínio.</summary>
    public const string AuthenticatedRead = "AuthenticatedRead";

    /// <summary>GET autenticado do perfil do utilizador (<c>/users/me</c>).</summary>
    public const string UserProfileRead = "UserProfileRead";

    /// <summary>
    /// GET de métricas do dashboard: TTL próprio, curto, com variação por utilizador.
    /// </summary>
    /// <remarks>
    /// Separada de <see cref="AuthenticatedRead"/> por causa do tempo de vida. As leituras de
    /// entidade toleram 90 minutos; um painel de métricas com essa idade mostraria "última
    /// atualização" de uma hora e meia atrás e o utilizador clicaria em atualizar sem efeito.
    /// A variação por utilizador é obrigatória: o escopo dos números depende de quem pergunta.
    /// </remarks>
    public const string DashboardRead = "DashboardRead";
}
