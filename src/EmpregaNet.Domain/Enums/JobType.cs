using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de vaga.
    /// </summary>
    public enum JobTypeEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Tempo Integral")] FullTime,
        [Description("Meio Período")] PartTime,
        [Description("Estágio")] Internship,
        [Description("Freelancer")] Freelancer,
        [Description("Temporário")] Temporary,
        [Description("Trainee")] Trainee,
        [Description("Voluntário")] Volunteer,
        [Description("Remoto")] Remote
    }
}