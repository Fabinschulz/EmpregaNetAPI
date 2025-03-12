using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o gênero.
    /// </summary>
    public enum Gender
    {
        [Description("Não informado")] NotInformed = 0,
        [Description("Masculino")] Male = 1,
        [Description("Feminino")] Female = 2
    }
}