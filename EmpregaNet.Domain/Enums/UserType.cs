using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de usuário (Candidato ou Recrutador).
    /// </summary>
    public enum UserType
    {
        [Description("Candidato")] Candidate = 0,
        [Description("Recrutador")] Recruiter = 1
    }
}