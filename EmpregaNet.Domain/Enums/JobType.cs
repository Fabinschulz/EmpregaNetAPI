using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de vaga.
    /// </summary>
    public enum JobType
    {
        [Description("Tempo Integral")] FullTime = 0,
        [Description("Meio Período")] PartTime = 1,
        [Description("Estágio")] Internship = 2,
        [Description("Freelancer")] Freelancer = 3,
        [Description("Temporário")] Temporary = 4,
        [Description("Trainee")] Trainee = 5,
        [Description("Voluntário")] Volunteer = 6,
        [Description("Remoto")] Remote = 7
    }
}