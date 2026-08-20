namespace EmpregaNet.Application.Users.ViewModel;

/// <summary>
/// Ficha do candidato para a equipe de recrutamento.
/// </summary>
public sealed class CandidateDetailViewModel
{
    public long Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }

    /// <summary>Descrição pt-BR do tipo de utilizador.</summary>
    public required string UserType { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];

    public string? ProfilePicture { get; init; }
    public string? City { get; init; }

    /// <summary>Sigla da UF; nulo quando o candidato não informou.</summary>
    public string? State { get; init; }

    public int? Age { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }

    public required CandidateApplicationsSummaryViewModel Applications { get; init; }
}
