using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o tipo de atividade da empresa.
    /// </summary>
    public enum TypeOfActivity
    {
        [Description("Indústria")] Industry = 0,
        [Description("Serviços")] services = 1,
        [Description("Comércio")] business = 2
    }
}