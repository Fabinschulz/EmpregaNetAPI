using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Experiência exigida pela vaga, medida em tempo.
    /// </summary>
    public enum ExperienceLevelEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Sem experiência")] SemExperiencia,
        [Description("Até 1 ano")] AteUmAno,
        [Description("De 1 a 3 anos")] DeUmATresAnos,
        [Description("De 3 a 5 anos")] DeTresACincoAnos,
        [Description("Mais de 5 anos")] MaisDeCincoAnos
    }
}
