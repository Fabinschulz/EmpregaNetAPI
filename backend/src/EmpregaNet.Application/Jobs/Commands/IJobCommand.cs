namespace EmpregaNet.Application.Jobs.Commands;

/// <summary>
/// Forma comum dos comandos de escrita de vaga (criar/atualizar).
/// Enums viajam como nome (<c>"Clt"</c>, <c>"SegundoTurno"</c>), não como inteiro.
/// </summary>
public interface IJobCommand
{
    long CompanyId { get; }
    string Title { get; }
    string? Summary { get; }
    string Description { get; }
    string JobType { get; }
    string WorkModel { get; }
    string WorkShift { get; }
    string ExperienceLevel { get; }
    string Area { get; }
    string City { get; }
    string State { get; }
    decimal? SalaryMin { get; }
    decimal? SalaryMax { get; }
    bool SalaryDisclosed { get; }
    bool IsPcdFriendly { get; }
    IReadOnlyList<string>? Requirements { get; }
    IReadOnlyList<string>? Benefits { get; }
}
