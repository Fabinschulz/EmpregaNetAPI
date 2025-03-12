using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o estado civil de um usuário.
    /// </summary>
    public enum CivilStatus
    {
        [Description("Casado")] Married = 0,
        [Description("Viúvo")] Widowed = 1,
        [Description("Separado")] Separated = 2,
        [Description("Divorciado")] Divorced = 3,
        [Description("Solteiro")] Single = 4
    }
}