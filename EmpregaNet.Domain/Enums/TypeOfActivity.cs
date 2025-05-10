using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de atividade da empresa.
    /// </summary>
    public enum TypeOfActivity
    {
        [Description("")] NaoSelecionado = 0,
        [Description("Indústria")] Industry = 1,
        [Description("Serviços")] services = 2,
        [Description("Comércio")] business = 3
    }
}