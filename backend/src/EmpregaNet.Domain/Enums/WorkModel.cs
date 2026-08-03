using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Modalidade de trabalho da vaga: onde o trabalho é executado.
    /// </summary>
    public enum WorkModelEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Presencial")] OnSite,
        [Description("Híbrido")] Hybrid,
        [Description("Remoto")] Remote
    }
}
