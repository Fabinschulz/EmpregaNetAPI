namespace EmpregaNet.Application.Abstraction;

/// <summary>
/// Garante que o usuário atual pode gerenciar recursos (vagas) de uma empresa (admin global ou vínculo <c>User.EmployerCompanyId</c>).
/// </summary>
public interface IJobEmployerAccess
{
    Task EnsureCanManageCompanyAsync(long companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve o escopo da empresa do usuário atual (Admin vê a plataforma inteira, Recruiter/Manager só a própria empresa).
    /// </summary>
    Task<long?> ResolveCompanyScopeAsync(CancellationToken cancellationToken = default);
}
