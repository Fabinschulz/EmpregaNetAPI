using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Vínculo/jornada da vaga. Descreve <b>como</b> a pessoa é contratada, não onde trabalha. 
    /// Para descrever onde a pessoa trabalha, ou seja,
    /// a modalidade (presencial/híbrido/remoto) é <see cref="WorkModelEnum"/>.
    /// </summary>
    public enum JobTypeEnum
    {
        [Description("")] NaoSelecionado = 0,
        [Description("Tempo Integral")] FullTime = 1,
        [Description("Meio Período")] PartTime = 2,
        [Description("Estágio")] Internship = 3,
        [Description("Freelancer")] Freelancer = 4,
        [Description("Temporário")] Temporary = 5,
        [Description("Trainee")] Trainee = 6,
        [Description("Voluntário")] Volunteer = 7,

        // 8 - reservado (era Remote).

        [Description("CLT")] Clt = 9,
        [Description("PJ")] Pj = 10
    }
}
