using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o gênero.
    /// </summary>
    public enum GenderEnum
    {
        [Description("Não informado")] NotInformed,
        [Description("Masculino")] Male,
        [Description("Feminino")] Female
    }
}