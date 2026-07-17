using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    
    public enum UserTypeEnum
    {
        /// <summary>
        /// Tipo de usuário não selecionado.
        /// </summary>
        [Description("")] NaoSelecionado,

        /// <summary>
        /// Tipo de usuário Candidato.
        /// </summary>
        /// <remarks>
        /// Representa um usuário que está em busca de uma oportunidade de emprego.
        /// </remarks>
        [Description("Candidato")] Candidate,

        /// <summary>
        /// Tipo de usuário Recrutador.
        /// / </summary>
        /// <remarks>
        /// Representa um usuário que está recrutando candidatos para uma vaga de emprego.
        /// </remarks>
        [Description("Recrutador")] Recruiter,

        /// <summary>
        /// Tipo de usuário Administrador.
        /// </summary>
        /// <remarks>
        /// Representa um usuário que tem acesso total ao sistema e pode gerenciar todos os usuários e dados.
        /// </remarks>
        [Description("Administrador")] Admin,

        /// <summary>
        /// Tipo de usuário Gestor.
        /// </summary>
        /// <remarks>
        /// Recrutador com visão de gestão da área de recrutamento (métricas/analytics da equipe),
        /// além das capacidades de recrutamento. Não tem acesso administrativo total ao sistema.
        /// </remarks>
        [Description("Gestor")] Manager
    }
}