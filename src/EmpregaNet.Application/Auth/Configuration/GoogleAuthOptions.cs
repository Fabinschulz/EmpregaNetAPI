namespace EmpregaNet.Application.Auth.Configuration;

/// <summary>
/// Validação de tokens ID do Google Sign-In (client IDs OAuth permitidos).
/// </summary>
public sealed class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    /// <summary>Client IDs (Web, iOS, Android) que podem emitir o <c>id_token</c> aceite pela API.</summary>
    public string[] ClientIds { get; set; } = [];
}
