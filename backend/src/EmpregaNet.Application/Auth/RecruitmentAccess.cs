using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Auth;

/// <summary>
/// Garante, na camada de aplicação, que a operação é feita por perfil de recrutamento (defesa em profundidade além do [Authorize]).
/// </summary>
public static class RecruitmentAccess
{
    public static void EnsureRecruitmentStaff(IHttpCurrentUser currentUser)
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
        if (!RecruitmentRoleNames.IsRecruitmentStaff(roles))
        {
            throw new ValidationAppException(
                nameof(currentUser),
                "Apenas perfis de recrutamento (Admin, Recruiter ou Manager) podem executar esta operação.",
                DomainErrorEnum.MISSING_RESOURCE_PERMISSION);
        }
    }
}
