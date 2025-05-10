using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o estado civil de um usuário.
    /// </summary>
    public enum CivilStatus
    {
        [Description("")] NaoSelecionado = 0,
        [Description("Casado")] Married = 1,
        [Description("Viúvo")] Widowed = 2,
        [Description("Separado")] Separated = 3,
        [Description("Divorciado")] Divorced = 4,
        [Description("Solteiro")] Single = 5
    }
}