using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de usuário (Candidato ou Recrutador).
    /// </summary>
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
        /// Representa um usuário que tem acesso a funcionalidades de gerenciamento, mas não tem acesso total ao sistema.
        /// </remarks>
        [Description("Gestor")] Manager,

        /// <summary>
        /// Tipo de usuário Entrevistador.
        /// </summary>
        /// <remarks>
        /// Representa um usuário que tem acesso a funcionalidades de entrevista, mas não tem acesso total ao sistema.
        /// </remarks>
        [Description("Entrevistador")] Interviewer,

        /// <summary>
        /// Tipo de usuário Analista.
        /// </summary>
        /// <remarks>
        /// Representa um usuário que tem acesso a funcionalidades de análise, mas não tem acesso total ao sistema.
        /// </remarks>
        [Description("Analista")] Analyst
        
    }
}