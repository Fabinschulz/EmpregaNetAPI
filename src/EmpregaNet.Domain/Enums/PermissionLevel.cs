using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    public enum PermissionLevelEnum
    {
        /// <summary>
        /// Nenhuma permissão de acesso ao recurso
        /// </summary>
        /// <remarks>
        /// O usuário não tem permissão para visualizar ou interagir com este recurso.
        /// </remarks>
        [Description("Nenhum")] None,

        /// <summary>
        /// Acesso completo a todos os recursos do sistema
        /// </summary>
        /// <remarks>
        /// Permite todas as operações (CRUD) em qualquer recurso do sistema, independentemente de criação ou vínculo.
        /// </remarks>
        [Description("Tudo")] All,

        /// <summary>
        /// Acesso aos recursos associados ao grupo do usuário
        /// </summary>
        /// <remarks>
        /// Permite operações (CRUD) apenas em recursos vinculados ao grupo de trabalho do usuário.
        /// Exemplo: Um recrutador pode gerenciar apenas as vagas atribuídas à sua equipe/área.
        /// </remarks>
        [Description("Meu grupo")] MyGroup,

        /// <summary>
        /// Acesso restrito aos recursos criados pelo próprio usuário
        /// </summary>
        /// <remarks>
        /// Permite operações (CRUD) apenas em recursos criados pelo próprio usuário.
        /// Exemplo: Um recrutador pode gerenciar apenas as vagas que ele mesmo cadastrou.
        /// </remarks>
        [Description("Somente eu")] OnlyMine,
    }
}