using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Auth;

/// <summary>
/// Garante que a operação é executada por um administrador (além do [Authorize] na API).
/// </summary>
public static class AdministradorAccess
{
    public static void EnsureAdministrator(IHttpCurrentUser currentUser)
    {
        var ctx = currentUser.GetContextUser();
        if (ctx is null)
        {
            throw new ValidationAppException(
                nameof(currentUser),
                "Usuário autenticado não encontrado no contexto.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }

        var roles = ctx.UserToken.GetRoleNames();
        if (!roles.Contains(RecruitmentRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationAppException(
                nameof(currentUser),
                "Apenas administradores podem executar esta operação.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }
    }
}
