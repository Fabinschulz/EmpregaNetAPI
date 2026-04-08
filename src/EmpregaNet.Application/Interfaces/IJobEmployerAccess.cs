namespace EmpregaNet.Application.Interfaces;

/// <summary>
/// Garante que o usuário atual pode gerenciar recursos (vagas) de uma empresa (admin global ou vínculo <c>User.EmployerCompanyId</c>).
/// </summary>
public interface IJobEmployerAccess
{
    Task EnsureCanManageCompanyAsync(long companyId, CancellationToken cancellationToken = default);
}
