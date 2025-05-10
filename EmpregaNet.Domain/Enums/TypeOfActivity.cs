using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de atividade da empresa.
    /// </summary>
    public enum TypeOfActivityEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Indústria")] Industry,
        [Description("Serviços")] services,
        [Description("Comércio")] business
    }
}